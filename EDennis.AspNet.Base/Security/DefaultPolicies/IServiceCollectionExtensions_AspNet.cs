using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security.DefaultPolicies {
    public static class IServiceCollectionExtensions_AspNet {
        public static IMvcBuilder AddControllersWithDefaultPolicies(this IServiceCollection services,
            IConfiguration config, IWebHostEnvironment env) {
            var builder = services.AddControllers(options=> {
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
