using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class ScopePropertiesMiddleware {

        private readonly RequestDelegate _next;
        public ScopePropertiesMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ScopeProperties scopeProperties) {

            if (context.Request.Path.Value.Contains("swagger"))
                await _next(context);
            else {

                //only relevant during testing
                var cookies = context.Request.Cookies;
                if (cookies.TryGetValue(CachedTransactionOptions.COOKIE_KEY, out string transactionScope))
                    scopeProperties.Add(CachedTransactionOptions.COOKIE_KEY, transactionScope);

                //relevant during testing and production
                var headers = context.Request.Headers;
                if (headers.TryGetValue(PassthroughClaimsOptions.CLAIMS_HEADER, out StringValues passthroughClaims))
                    scopeProperties.Add(PassthroughClaimsOptions.CLAIMS_HEADER, passthroughClaims.ToString());
 
                await _next(context); 
            }

        }

    }

    public static class IApplicationBuilderExtensions_ScopePropertiesMiddleware {
        public static IApplicationBuilder UseScopeProperties(this IApplicationBuilder app) {
            app.UseMiddleware<ScopePropertiesMiddleware>();
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
