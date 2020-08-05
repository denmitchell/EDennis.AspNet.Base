using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {
    public static class IServiceCollectionExtensions_DefaultPolicies {
        public static IMvcCoreBuilder AddControllersWithDefaultPolicies(this IServiceCollection services,
            IConfiguration config, IHostEnvironment env) {
            var builder = services.AddMvcCore(options=> {
                options.Conventions.Add(new DefaultAuthorizationPolicyConvention(env.ApplicationName, config));
            });

            services.AddSingleton<IAuthorizationPolicyProvider>((container) => {
                var logger = container.GetRequiredService<ILogger<DefaultPoliciesAuthorizationPolicyProvider>>();
                return new DefaultPoliciesAuthorizationPolicyProvider(
                    config, logger);
            });

            return builder;
        }
    }
}
