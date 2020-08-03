using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace EDennis.NetStandard.Base {
    public class HttpLoggingMiddleware {

        private readonly RequestDelegate next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly string _configFileDir;
        private readonly string _configFileName = "httpLogging.json";
        private HttpLoggingOptions _options;

        public HttpLoggingMiddleware(RequestDelegate next, 
            ILogger<HttpLoggingMiddleware> logger, 
            IConfiguration config) {
            this.next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
            _configFileDir = config.GetValue<string>(WebHostDefaults.ContentRootKey);
            _configFileName = config["HttpLogging:ConfigFile"] ?? _configFileName;

            //initial load of configuration file
            LoadConfigurationAsync().Wait();

            //initiate file watcher for changes in configuration.
            Task.Run(() => WatchConfigFileAsync());
        }

        public async Task WatchConfigFileAsync() {
            await Task.Run(()=>{
                using var watcher = new FileSystemWatcher {
                    Path = _configFileDir,
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = _configFileName,
                    IncludeSubdirectories = false
                };

                // Add event handlers.
                watcher.Changed += OnChangedAsync;

                // Begin watching.
                watcher.EnableRaisingEvents = true;
            });
        }


        // Define the event handlers.
        private async void OnChangedAsync(object source, FileSystemEventArgs e) {
            await LoadConfigurationAsync();
        }

        private async Task LoadConfigurationAsync() {
            await Task.Run(() =>
            {
                try {
                    var config = new ConfigurationBuilder()
                        .AddJsonFile($"{_configFileDir}/{_configFileName}")
                        .Build();
                    _options = new HttpLoggingOptions();
                    config.Bind(_options);
                } catch (Exception ex) {
                    _logger.LogError($"Could not load HttpLogging configuration file at {_configFileDir}/{_configFileName}: " + ex.Message);
                }

            });
        }



        public async Task InvokeAsync(HttpContext context) {

            _logger.LogInformation($"*****PATH: {context.Request.Path.Value}");

            if (context.Request.Path.Value.Contains("swagger")
                || !_options.Enabled 
                || (!MatchesQuery(context) && !MatchesClaim(context)))
                await next.Invoke(context);


            // create a new log object
            var log = new HttpLog {
                Path = context.Request.Path,
                Method = context.Request.Method,
                QueryString = context.Request.QueryString.ToString(),
                DisplayUrl = context.Request.GetDisplayUrl(),
                Headers = _options.IncludeHeaders ? context.Request.Headers.ToDictionary((KeyValuePair<string, StringValues> h)=> (h.Key, h.Value.ToString())) : null,
                Claims = _options.IncludeClaims ? context.Request.HttpContext.User?.Claims?.ToDictionary((Claim c) => (c.Type, c.Value)) : null
            };

            if (_options.MaxRequestBodyLength > 0                
                && context.Request.ContentLength != default
                && context.Request.ContentLength > 0
                && (context.Request.Method == "POST" || context.Request.Method == "PUT" 
                || context.Request.Method == "PATCH") 
                ) { 
                context.Request.EnableBuffering();
                var body = await new StreamReader(context.Request.Body)
                                                    .ReadToEndAsync();
                context.Request.Body.Position = 0;
                log.Payload = body.SafeSubstring(0,_options.MaxRequestBodyLength);
            }

            log.RequestedOn = DateTime.Now;

            if (_options.MaxResponseBodyLength == 0)
                await next.Invoke(context);
            else {
                var originalBodyStream = context.Response.Body;

                using var responseBody = _recyclableMemoryStreamManager.GetStream();
                context.Response.Body = responseBody;

                await next.Invoke(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                log.Response = await new StreamReader(context.Response.Body).ReadToEndAsync();
                log.Response = log.Response.SafeSubstring(0, _options.MaxResponseBodyLength);
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                await responseBody.CopyToAsync(originalBodyStream);
            }

            log.ResponseCode = context.Response.StatusCode.ToString();

            using (_logger.BeginScope(new Dictionary<string, object> {
                            { "Path", log.Path },
                            { "QueryString", log.QueryString },
                            { "DisplayUrl", log.DisplayUrl },
                            { "Method", log.Method },
                            { "Headers", log.Headers },
                            { "Claims", log.Claims },
                            { "RequestedOn", log.RequestedOn },
                            { "RespondedOn", log.RespondedOn },
                            { "ResponseCode", log.ResponseCode },
                            { "RequestBody", log.Payload },
                            { "ResponseBody", log.Response }})) {
                _logger.LogInformation("Http Request and Response logged for {log.DisplayUrl}", log.DisplayUrl);
            }



        }


        private bool MatchesClaim(HttpContext context) {
            var user = context.User;
            if (user == null)
                return false;

            foreach (var entry in _options.ForClaims) {
                if (user.Claims.Any(c => c.Type == entry.Key && entry.Value.Contains(c.Value)))
                    return true;
            }

            return false;
        }

        private bool MatchesQuery(HttpContext context) {
            var query = context.Request.Query;
            if (query == null)
                return false;

            if (query.ContainsKey(_options.ForQueryKey))
                return true;

            return false;
        }


    }

    public static class IApplicationBuilderExtensions {
        public static IApplicationBuilder UseHttpLogging(this IApplicationBuilder app) {
            app.UseMiddleware<HttpLoggingMiddleware>();
            return app;
        }

        public static IApplicationBuilder UseHttpLoggingFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context =>
                {
                    foreach (var partialPath in startsWithSegments)
                        if (context.Request.Path.StartsWithSegments(partialPath))
                            return true;
                    return false;
                }, 
                app => app.UseHttpLogging()
            );
            return app;
        }

        public static IApplicationBuilder UseHttpLoggingWhen(this IApplicationBuilder app,
            Func<HttpContext,bool> predicate) {
                app.UseWhen(predicate, app => app.UseHttpLogging());
            return app;
        }


    }

}
