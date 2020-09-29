using EDennis.NetStandard.Base;
using IdentityModel.AspNetCore;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProxyKit;

namespace EDennis.NetApp.Base {
    public static class IApplicationBuilderExtensions {

        public static IApplicationBuilder UseSecureForwarding(this IApplicationBuilder app,
            IConfiguration config, string apisConfigKey
            ) {

            app.UseMiddleware<StrictSameSiteExternalAuthenticationMiddleware>();
            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                if (!context.User.Identity.IsAuthenticated) {
                    await context.ChallengeAsync();
                    return;
                }

                await next();
            });

            var apis = new Apis();
            config.GetSection(apisConfigKey).Bind(apis);

            foreach (var api in apis) {
                app.Map($"/{api.Key}", app2 =>
                {
                    app2.RunProxy(async context =>
                    {
                        var forwardContext = context.ForwardTo(api.Value);

                        var token = await context.GetUserAccessTokenAsync();
                        forwardContext.UpstreamRequest.SetBearerToken(token);

                        return await forwardContext.Send();
                    });
                });
            }

            return app;

        }
    }
}
