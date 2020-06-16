using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base {
    public class OidcLoggingMiddleware {

        private readonly RequestDelegate _next;

        public static Regex RequestVerificationTokenRegEx = new Regex("(?<=<input\\s+name\\s*=\\s*\"__RequestVerificationToken\"\\s+type\\s*=\\s*\"hidden\"\\s+value\\s*=\\s*\")[A-Za-z0-9_-]+");

        private static string[] TargetEndpoints = new string[] {
            "/openid-configuration","/_configuration/",
            "/connect/token","/connect/authorize",
            "/connect/userinfo","/connect/endsession",
            "/connect/revocation","/connect/introspect",
            "/connect/deviceauthorization",
            "/Identity/Account/Login"
        };

        ILogger<OidcLoggingMiddleware> _logger;
        IOptionsMonitor<OidcLoggingOptions> _options;

        public OidcLoggingMiddleware (RequestDelegate next, 
            IOptionsMonitor<OidcLoggingOptions> options,
            ILogger<OidcLoggingMiddleware> logger) {
            _next = next;
            _logger = logger;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context) {
            if (!_options.CurrentValue.Enabled
                || ( !TargetEndpoints.Any(x => context.Request.Path.Value.Contains(x))
                    && !context.Request.Headers.ContainsKey("Authorization")
                    ))
                await _next(context);
            else {
                var url = UriHelper.GetDisplayUrl(context.Request);

                context.Request.EnableBuffering();

                var requestBodyStream = new MemoryStream();
                var originalRequestBody = context.Request.Body;

                await context.Request.Body.CopyToAsync(requestBodyStream);
                requestBodyStream.Seek(0, SeekOrigin.Begin);

                var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
                _logger.LogInformation($@"
<REQUEST method='{ context.Request.Method}'>
     <URL>
{IndentAll(url, 80, 10)}
     </URL>{GetHeaders(context.Request)}{FormatBody(requestBodyText)}
</REQUEST>
");


                requestBodyStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = requestBodyStream;

                var path = context.Request.Path.Value;
                if (string.IsNullOrWhiteSpace(context.Response.ContentType) ||
                        context.Response.ContentType == "application/json" ||
                           path.Contains("/Account/Login"))
                    await LogResponse(context, _next);
                else
                    await _next(context);
            }

        }



        private string GetHeaders(HttpRequest req) {
            var sb = new StringBuilder();
            foreach (var hdr in req.Headers.Where(h =>
                h.Key.Contains("Authorization") || h.Key == "Cookie" || h.Key == "Referer")) {
                sb.Append($"{hdr.Key}: {hdr.Value}" + '\u001E');
            }
            if (!string.IsNullOrWhiteSpace(sb.ToString()))
                return $@"
     <HEADERS>
{IndentAll(sb.ToString(), 80, 10)}     
     </HEADERS>";
            else
                return "";
        }

        private string GetHeaders(HttpResponse resp) {
            var sb = new StringBuilder();
            foreach (var hdr in resp.Headers.Where(h =>
                h.Key == "Location" || h.Key == "Set-Cookie")) {
                sb.Append($"{hdr.Key}: {hdr.Value}" + '\u001E');
            }
            if (!string.IsNullOrWhiteSpace(sb.ToString()))
                return $@"
     <HEADERS>
{IndentAll(sb.ToString(), 80, 10)}     
     </HEADERS>";
            else
                return sb.ToString();
        }


        public string FormatBody(string body) {
            if (body != null && body.Length > 0)
                return $@"
     <BODY>
{IndentAll(body, 80, 10)}
     </BODY>";
            else
                return "";
        }

        private async Task LogResponse(HttpContext context, RequestDelegate next) {
            var originalBodyStream = context.Response.Body;
            var rmsm = new RecyclableMemoryStreamManager();
            await using var responseBody = rmsm.GetStream();
            context.Response.Body = responseBody;
            await next(context);
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);


            if (context.Request.Path.Value.Contains("/Account/Login")) {
                text = RequestVerificationTokenRegEx.Match(text).Value;
                _logger.LogInformation(@$"
<RESPONSE statusCode='{context.Response.StatusCode}'>{(context.Response.StatusCode == 302 ? "\n<LOCATION><![CDATA[" + context.Response.Headers["Location"].ToString() + "]]></LOCATION>" : "")}
     <URL>
{IndentAll(context.Request.Path.Value, 80, 10)}
     </URL>{GetHeaders(context.Response)}{FormatBody($"RequestVerificationToken>{text}</__RequestVerificationToken")}
</RESPONSE>");
            } else {


                _logger.LogInformation(@$"
<RESPONSE statusCode='{context.Response.StatusCode}'>
     <URL>
{IndentAll(context.Request.Path.Value, 80, 10)}
     </URL>{GetHeaders(context.Response)}{FormatBody(text)}
</RESPONSE>
");
            }
            await responseBody.CopyToAsync(originalBodyStream);
        }

        private string IndentAll(string text, int columnWidth, int spaces) {
            var complete = new StringBuilder();
            var line = new StringBuilder();
            for (int i = 0; i < text.Length; i++) {
                if (text[i] != +'\u001E')
                    line.Append(text[i]);
                if (i == text.Length - 1) {
                    complete.Append(new string(' ', spaces) + line.ToString());
                } else if (text[i] == +'\u001E') {
                    complete.AppendLine(new string(' ', spaces) + line.ToString());
                    line.Clear();
                } else if (line.Length == columnWidth) {
                    complete.AppendLine(new string(' ', spaces) + line.ToString());
                    line.Clear();
                }
            }
            return complete.ToString();
        }
    }


    public static class IServiceCollectionExtensions_OidcLoggingMiddleware {
        public static IServiceCollection AddOidcLogging(this IServiceCollection services, IConfiguration config) {
            services.Configure<OidcLoggingOptions>(config.GetSection("Logging:OidcLogging"));
            return services;
        }
    }

    public static class IApplicationBuilderExtensions_OidcLoggingMiddleware {
        public static IApplicationBuilder UseOidcLogging(this IApplicationBuilder app) {
            app.UseMiddleware<OidcLoggingMiddleware>();
            return app;
        }
    }

}
