using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Copies designated cookies and headers to a ScopedRequestMessage object, which can be used
    /// by an QueryApiClient or CrudApiClient as the base HttpRequestMessage.
    /// NOTE: To pass claims through an API client to a child API, first invoke the
    /// ClaimsToHeader middleware.
    /// </summary>
    public class ScopedRequestMiddleware {

        private readonly RequestDelegate _next;
        private readonly ILogger<ScopedRequestMessage> _logger;
        private readonly ScopedRequestMessageOptions _options;

        public ScopedRequestMiddleware(RequestDelegate next, 
            IOptionsMonitor<ScopedRequestMessageOptions> options, 
            ILogger<ScopedRequestMessage> logger) {
            _options = options.CurrentValue;
            _next = next;
            _logger = logger;

            _logger.LogDebug("ScopedRequestMiddleware constructed with {@ScopedRequestMessageOptions}", _options);

        }

        public async Task InvokeAsync(HttpContext context, ScopedRequestMessage scopedRequestMessage) {


            using (_logger.BeginScope("ScopedRequestMiddleware executing for user with Claims: {@Claims}.", context.User.Claims)) {

                var cookies = context.Request.Cookies
                .Where(c => _options.CookiesToCapture.Contains(c.Key, StringComparer.OrdinalIgnoreCase))
                .ToList();

                _logger.LogDebug("Adding {@Cookies}", cookies);

                foreach (var cookie in cookies)
                    scopedRequestMessage.AddCookie(cookie.Key, cookie.Value);


                var headers = context.Request.Headers
                    .Where(h => h.Key == CachedTransactionOptions.COOKIE_KEY 
                        || _options.HeadersToCapture.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                _logger.LogDebug("Adding {@Headers}", headers);

                foreach(var header in headers)
                    scopedRequestMessage.AddHeader(header.Key, header.Value);

            }
            
            await _next(context); 

        }

    }

    public static class IServiceCollectionExtensions_ScopeddRequestMessageMiddleware {
        public static IServiceCollection AddScopedRequestMessage(this IServiceCollection services, IConfiguration config,
            string configkey = "ScopedRequestMessage") {                       
            services.Configure<ScopedRequestMessageOptions>(config.GetSection(configkey));

            services.AddScoped<ScopedRequestMessage>();
            return services;
        }
    }


    public static class IApplicationBuilderExtensions_ScopedRequestMessageMiddleware {
        public static IApplicationBuilder UseScopedRequestMessage(this IApplicationBuilder app) {
            app.UseMiddleware<ScopedRequestMiddleware>();
            return app;
        }


        public static IApplicationBuilder UseScopedRequestMessageFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context =>
            {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseScopedRequestMessage()
            );
            return app;
        }

        public static IApplicationBuilder UseScopedRequestMessageWhen(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate) {
            app.UseWhen(predicate, app => app.UseScopedRequestMessage());
            return app;
        }


    }

}
