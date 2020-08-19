using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ScopedRequestMessageOptions _options;

        public ScopedRequestMiddleware(RequestDelegate next, IOptionsMonitor<ScopedRequestMessageOptions> options) {
            _options = options.CurrentValue;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ScopedRequestMessage scopedRequestMessage,
            IConfiguration config) {

            context.Request.Cookies
                .Where(c => _options.CookiesToCapture.Contains(c.Key, StringComparer.OrdinalIgnoreCase))
                .ToList()
                .ForEach(c => scopedRequestMessage.AddCookie(c.Key, c.Value));

            context.Request.Headers
                .Where(h => _options.HeadersToCapture.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
                .ToList()
                .ForEach(h => scopedRequestMessage.AddHeader(h.Key, h.Value));
 
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
