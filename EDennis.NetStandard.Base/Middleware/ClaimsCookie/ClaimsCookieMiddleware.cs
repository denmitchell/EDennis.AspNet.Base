using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class ClaimsCookieMiddleware {

        public const string COOKIE_KEY = "MyClaims";

        private readonly RequestDelegate _next;

        public ClaimsCookieMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context) {

            var cookieValue = GetOrAddCookie(context, out bool cookieAdded);

            if (cookieAdded) {
                context.Request.Headers.Add(COOKIE_KEY, cookieValue);
                context.Response.OnStarting(state => {
                    var httpContext = (HttpContext)state;
                    httpContext.Response.Cookies.Append(COOKIE_KEY, cookieValue);
                    return Task.CompletedTask;
                }, context);
            }

            await _next(context);

        }




        private string GetOrAddCookie(HttpContext context, out bool added) {
            var claims = context.User?.Claims;
            if (context.Request.Cookies.TryGetValue(COOKIE_KEY, out string cookieValue)) {
                added = false;
                return cookieValue;
            } else if (claims != null) {
                var json = JsonSerializer.Serialize(claims.Select(c => new { c.Type, c.Value }).ToList());
                added = true;
                return json;
            } else {
                added = false;
                return null;
            }
        }


    }

    public static class IApplicationBuilderExtensions_UserClaimsMiddleware {

        public static IApplicationBuilder UseClaimsCookie(this IApplicationBuilder app) {
            app.UseMiddleware<ClaimsCookieMiddleware>();
            return app;
        }


        public static IApplicationBuilder UseClaimsCookieFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context => {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseClaimsCookie()
            );
            return app;
        }

        public static IApplicationBuilder UseClaimsCookieWhen(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate) {
            app.UseWhen(predicate, app => app.UseClaimsCookie());
            return app;
        }




    }

}
