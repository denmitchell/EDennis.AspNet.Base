using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Writers;
using System.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    public class HeaderToClaimsMiddleware {
        private readonly RequestDelegate _next;
        private readonly HeaderToClaimsOptions _settings;

        public HeaderToClaimsMiddleware(RequestDelegate next,
            IOptionsMonitor<HeaderToClaimsOptions> settings) {
            _next = next;
            _settings = settings.CurrentValue;
        }

        public async Task InvokeAsync(HttpContext context, ScopeProperties scopeProperties,
            ILogger<ClaimsToHeaderMiddleware> logger) {

            var req = context.Request;
            var enabled = _settings.Enabled;

            if (!enabled) {
                await _next(context);
            } else {

                if (!context.User.Identity.IsAuthenticated) {
                    var ex = new SecurityException($"Cannot invoke HeaderToClaimsMiddleware with unauthenticated user");
                    logger.LogError(ex, ex.Message);
                    throw ex;
                }


                if (req.Headers.TryGetValue(HeaderToClaimsOptions.HEADER_KEY, out StringValues value)) {
                    var unpackedClaims = value.ToString().UnpackKeyValues((t, v) => new Claim(t, v));

                    var appIdentity = new ClaimsIdentity(unpackedClaims);
                    context.User.AddIdentity(appIdentity);
                }


                await _next(context);
            }
        }
    }


    public static class IServiceCollectionExtensions_HeaderToClaimsMiddleware {
        public static IServiceCollection AddHeaderToClaims(this IServiceCollection services, IConfiguration config) {
            services.TryAddScoped<ScopeProperties>();
            services.Configure<MockUserOptions>(config.GetSection("Security:HeaderToClaims"));
            return services;
        }
    }


    public static partial class IApplicationBuilderExtensions_HeaderToClaimsMiddleware {
        public static IApplicationBuilder UseHeaderToClaims(this IApplicationBuilder app) {
            app.UseMiddleware<HeaderToClaimsMiddleware>();
            return app;
        }


        public static IApplicationBuilder UseHeaderToClaimsFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context =>
            {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseHeaderToClaims()
            );
            return app;
        }

        public static IApplicationBuilder UseHeaderToClaimsWhen(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate) {
            app.UseWhen(predicate, app => app.UseHeaderToClaims());
            return app;
        }

    }

}
