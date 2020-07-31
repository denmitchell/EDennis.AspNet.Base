using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// This middleware will transform headers into user claims per configuration settings.
    /// 
    /// This middleware is designed to be used immediately BEFORE or AFTER UseAuthentication() or
    /// UseAuthorization().  All header/claims configured for PostAuthentication will be ignored if
    /// the User is not authenticated.
    /// 
    /// When used after UseAuthorization, the claims are merely extra claims that can be used for
    /// purposes other than all-or-nothing access to a protected resource (e.g., using a claim's
    /// value to filter data requests.)
    /// </summary>
    public class PassthroughClaimsMiddleware {
        private readonly RequestDelegate _next;
        private readonly PassthroughClaimsOptions _settings;

        public PassthroughClaimsMiddleware(RequestDelegate next,
            IOptionsMonitor<PassthroughClaimsOptions> settings) {
            _next = next;
            _settings = settings.CurrentValue;
        }

        public async Task InvokeAsync(HttpContext context) {

            var req = context.Request;
            var enabled = _settings.Enabled;

            if (!enabled || req.Path.StartsWithSegments(new PathString("/swagger"))) {
                await _next(context);
            } else {

                if (context.User != null
                        && req.Headers.TryGetValue(PassthroughClaimsOptions.CLAIMS_HEADER, out StringValues value)) {
                    var claims = value.ToString().UnpackKeyValues((t, v) => new Claim(t, v));
                    var appIdentity = new ClaimsIdentity(claims);
                    context.User.AddIdentity(appIdentity);
                }

                await _next(context);
            }
        }
    }


    public static partial class IApplicationBuilderExtensions_Middleware {
        public static IApplicationBuilder UsePassthroughClaims(this IApplicationBuilder app) {
            app.UseMiddleware<PassthroughClaimsMiddleware>();
            return app;
        }
    }

}
