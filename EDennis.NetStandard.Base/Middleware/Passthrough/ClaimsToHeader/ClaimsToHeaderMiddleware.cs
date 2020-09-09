using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// This middleware will transform headers into user claims per configuration settings.
    /// 
    /// The middleware requires a ClaimTypesConfigKey, which is an array of claim types to
    /// pack into the header.  This key could point to Security:OpenIdConnect:Scope, which
    /// should have all the relevant user claims.
    /// 
    /// This middleware is designed to be used immediately BEFORE or AFTER UseAuthentication() or
    /// UseAuthorization().  All header/claims configured for PostAuthentication will be ignored if
    /// the User is not authenticated.
    /// 
    /// When used after UseAuthorization, the claims are merely extra claims that can be used for
    /// purposes other than all-or-nothing access to a protected resource (e.g., using a claim's
    /// value to filter data requests.)
    /// </summary>
    public class ClaimsToHeaderMiddleware {
        private readonly RequestDelegate _next;
        private readonly ClaimsToHeaderOptions _settings;
        private readonly List<string> _claimTypes = new List<string>();

        public ClaimsToHeaderMiddleware(RequestDelegate next,
            IOptionsMonitor<ClaimsToHeaderOptions> settings,
            IConfiguration config) {
            _next = next;
            _settings = settings.CurrentValue;
            config.BindSectionOrThrow(_settings.ClaimTypesConfigKey,_claimTypes);
        }

        public async Task InvokeAsync(HttpContext context,
            ILogger<ClaimsToHeaderMiddleware> logger) {

            var req = context.Request;
            var enabled = _settings.Enabled;

            if (!enabled) {
                await _next(context);
            } else {

                if (!context.User.Identity.IsAuthenticated) {
                    var ex = new SecurityException($"Cannot invoke ClaimsToHeaderMiddleware with unauthenticated user");
                    logger.LogError(ex, ex.Message);
                    throw ex;
                }

                var packedClaims = context.User.Claims
                    .Where(c=>_claimTypes.Contains(c.Type))
                    .PackKeyValues(c => (c.Type, c.Value));

                if (req.Headers.ContainsKey(HeaderToClaimsOptions.HEADER_KEY))
                    req.Headers[HeaderToClaimsOptions.HEADER_KEY] = packedClaims;
                else
                    req.Headers.Add(HeaderToClaimsOptions.HEADER_KEY, packedClaims);


                await _next(context);
            }
        }
    }


    public static class IServiceCollectionExtensions_ClaimsToHeaderMiddleware {
        public static IServiceCollection AddClaimsToHeader(this IServiceCollection services, IConfiguration config,
            string configKey = "Security:ClaimsToHeader") {
            services.Configure<MockClaimsPrincipalOptions>(config.GetSection(configKey));
            return services;
        }
    }


    public static partial class IApplicationBuilderExtensions_ClaimsToHeaderMiddleware {
        public static IApplicationBuilder UseClaimsToHeader(this IApplicationBuilder app) {
            app.UseMiddleware<ClaimsToHeaderMiddleware>();
            return app;
        }


        public static IApplicationBuilder UseClaimsToHeaderFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context =>
            {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseClaimsToHeader()
            );
            return app;
        }

        public static IApplicationBuilder UseClaimsToHeaderWhen(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate) {
            app.UseWhen(predicate, app => app.UseClaimsToHeader());
            return app;
        }

    }

}
