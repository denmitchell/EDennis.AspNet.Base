using IdentityModel.AspNetCore;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ProxyKit;
using System;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.Extensions {
    public static class IApplicationBuilderExtensions {

        public static IApplicationBuilder UseSecureForwarding(this IApplicationBuilder app,
            Func<HttpContext,bool,Task<string>> getUserAccessTokenAsyncMethod,
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

                        var token = await getUserAccessTokenAsyncMethod(context, false);
                        forwardContext.UpstreamRequest.SetBearerToken(token);

                        return await forwardContext.Send();
                    });
                });
            }

            return app;

        }
    }
}
