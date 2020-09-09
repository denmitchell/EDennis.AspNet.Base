using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EDennis.NetStandard.Base {

    public static class IServiceCollectionExtensions_Security {

        /// <summary>
        /// Configures a singleton cache that can be used to resolve
        /// parent claims to child claims.  In addition to configuring
        /// DI for the ChildClaimCache and its Options (ChildClaimSettings) 
        /// from configuration, it also sets up DI for IClaimsTransformation,
        /// where the implementation is a ChildClaimsTransformer.
        /// </summary>
        /// <param name="services">IServiceCollection instance</param>
        /// <param name="config">IConfiguration instance</param>
        /// <param name="configKey">configuration key for ChildClaimSettings</param>
        /// <returns></returns>
        public static IServiceCollection AddChildClaimCache(this IServiceCollection services, 
            IConfiguration config, string configKey = "Security:ChildClaims") {

            services.Configure<ChildClaimSettings>(config.GetSection(configKey));
            services.AddSingleton<IChildClaimCache,ChildClaimCache>();
            services.AddScoped<IClaimsTransformation, ChildClaimsTransformer>();

            return services;
        }



        /// <summary>
        /// Sets up the SecureTokenService and BearerTokenHandler via
        /// <code>
        /// services.AddSecureTokenService<ClientCredentialsTokenService>(Configuration);
        /// </code>
        /// Note: you need to include a Security:ClientCredentials section in 
        /// configuration.
        /// Note: you need to call app.UseAuthentication in Configure, as well.
        /// </summary>
        /// <typeparam name="TTokenService"></typeparam>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static IServiceCollection AddSecureTokenService<TTokenService>(this IServiceCollection services,
            IConfiguration config, string configKey = "Security:ClientCredentials")
            where TTokenService : class, ITokenService {

            services.AddSingleton<ITokenService, TTokenService>();

            if (typeof(ClientCredentialsTokenService).IsAssignableFrom(typeof(TTokenService))) {

                var options = new ClientCredentialsOptions();
                var configSection = config.GetSection(configKey);
                configSection.Bind(options);

                services.Configure<ClientCredentialsOptions>(opt =>
                {
                    opt.Authority = options.Authority;
                    opt.ClientId = options.ClientId;
                    opt.ClientSecret = options.ClientSecret;
                    opt.Scopes = options.Scopes;
                });

                if (options.Authority == null)
                    throw new Exception($"Not able to bind Configuration[\"{configKey}\"] to ClientCredentialsOptions");

                services.AddHttpClient("ClientCredentialsTokenService", configure =>
                {
                    configure.BaseAddress = new Uri(options.Authority);
                });

                //TODO: See if this is needed
                //services.AddAuthentication("Bearer")
                //    .AddScheme<BearerTokenOptions, BearerTokenHandler>("Bearer", opt => { });
            }

            return services;
        }

        public const string PROXY_CLIENTS_CONFIG_KEY_PARENT = "ProxyClients";
        public const string API_CLIENTS_CONFIG_KEY_PARENT = "ApiClients";

        public static TypedClientBuilder AddProxyClient(this IServiceCollection services,
                IConfiguration config, string clientName, string configKeyParent = PROXY_CLIENTS_CONFIG_KEY_PARENT) {

            var builder = new TypedClientBuilder { ConfigKeyParent = configKeyParent, Configuration = config, Services = services };
            builder.AddProxyClient(clientName);

            return builder;
        }


        public static TypedClientBuilder AddApiClient<TClientImplementation>(this IServiceCollection services,
                IConfiguration config, string clientName = null, string configKeyParent = API_CLIENTS_CONFIG_KEY_PARENT)
            where TClientImplementation : class
                => AddApiClient<TClientImplementation, TClientImplementation>(services, config, configKeyParent, clientName);


        public static TypedClientBuilder AddApiClient<TClientInterface, TClientImplementation>(this IServiceCollection services,
                IConfiguration config, string configKeyParent = API_CLIENTS_CONFIG_KEY_PARENT, string clientName = null )
            where TClientInterface : class
            where TClientImplementation : class, TClientInterface {

            var builder = new TypedClientBuilder { ConfigKeyParent = configKeyParent, Configuration = config, Services = services };
            return builder.AddApiClient<TClientInterface, TClientImplementation>(clientName);
        }

    }
}
