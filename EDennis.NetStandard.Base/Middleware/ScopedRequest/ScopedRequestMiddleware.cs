using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
        private readonly RequestForwardingOptions _options;

        public ScopedRequestMiddleware(RequestDelegate next, IOptionsMonitor<RequestForwardingOptions> options) {
            _options = options.CurrentValue;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ScopedRequestMessage scopedRequestMessage) {

            context.Request.Cookies
                .Where(c => _options.CookiesToForward.Contains(c.Key, StringComparer.OrdinalIgnoreCase))
                .ToList()
                .ForEach(c => scopedRequestMessage.AddCookie(c.Key, c.Value));

            context.Request.Headers
                .Where(h => _options.HeadersToForward.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
                .ToList()
                .ForEach(h => scopedRequestMessage.AddHeader(h.Key, h.Value));
 
            await _next(context); 

        }

    }

    public static class IApplicationBuilderExtensions_ScopePropertiesMiddleware {
        public static IApplicationBuilder UseScopeProperties(this IApplicationBuilder app) {
            app.UseMiddleware<ScopedRequestMiddleware>();
            return app;
        }


        public static IApplicationBuilder UseScopePropertiesFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context =>
            {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseScopeProperties()
            );
            return app;
        }

        public static IApplicationBuilder UseScopePropertiesWhen(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate) {
            app.UseWhen(predicate, app => app.UseScopeProperties());
            return app;
        }


    }

}
