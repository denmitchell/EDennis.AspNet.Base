using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EDennis.NetStandard.Base {


    public static class IServiceCollectionExtensions_Security {


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
        /// <param name="tokenServiceConfigKey"></param>
        /// <returns></returns>
        public static IServiceCollection AddSecureTokenService<TTokenService>(this IServiceCollection services,
            IConfiguration config, string tokenServiceConfigKey = "Security:ClientCredentials")
            where TTokenService : class, ITokenService {

            services.AddSingleton<ITokenService, TTokenService>();

            if (typeof(ClientCredentialsTokenService).IsAssignableFrom(typeof(TTokenService))) {

                var options = new ClientCredentialsOptions();
                var configSection = config.GetSection(tokenServiceConfigKey);
                configSection.Bind(options);

                services.Configure<ClientCredentialsOptions>(opt =>
                {
                    opt.Authority = options.Authority;
                    opt.ClientId = options.ClientId;
                    opt.ClientSecret = options.ClientSecret;
                    opt.Scopes = options.Scopes;
                });

                if (options.Authority == null)
                    throw new Exception($"Not able to bind Configuration[\"{tokenServiceConfigKey}\"] to ClientCredentialsOptions");

                services.AddHttpClient("ClientCredentialsTokenService", configure =>
                {
                    configure.BaseAddress = new Uri(options.Authority);
                });

                services.AddAuthentication("Bearer")
                    .AddScheme<BearerTokenOptions, BearerTokenHandler>("Bearer", opt => { });
            }

            return services;
        }

        private const string DEFAULT_DEPENDPOINTS_CONFIG_KEY = "DepEndpoints";


        public static TypedClientBuilder AddProxyClient<TClientImplementation>(this IServiceCollection services,
                IConfiguration config, string configKey = DEFAULT_DEPENDPOINTS_CONFIG_KEY)
            where TClientImplementation : class
                => AddApiClient<TClientImplementation, TClientImplementation>(services, config, configKey);


        public static TypedClientBuilder AddProxyClient<TClientInterface, TClientImplementation>(this IServiceCollection services,
                IConfiguration config, string configKey = DEFAULT_DEPENDPOINTS_CONFIG_KEY)
            where TClientInterface : class
            where TClientImplementation : class, TClientInterface
                => AddApiClient<TClientInterface, TClientImplementation>(services, config, configKey);


        public static TypedClientBuilder AddApiClient<TClientImplementation>(this IServiceCollection services,
                IConfiguration config, string configKey = DEFAULT_DEPENDPOINTS_CONFIG_KEY)
            where TClientImplementation : class
                => AddApiClient<TClientImplementation, TClientImplementation>(services, config, configKey);



        public static TypedClientBuilder AddApiClient<TClientInterface, TClientImplementation>(this IServiceCollection services,
                IConfiguration config, string configKey = DEFAULT_DEPENDPOINTS_CONFIG_KEY)
            where TClientInterface : class
            where TClientImplementation : class, TClientInterface {

            configKey ??= DEFAULT_DEPENDPOINTS_CONFIG_KEY;
            var dependPoints = new DepEndpoints();
            config.BindSectionOrThrow(configKey, dependPoints);

            var clientBuilder = new TypedClientBuilder { DepEndpoints = dependPoints, Services = services };

            clientBuilder.AddClient<TClientInterface, TClientImplementation>();

            return clientBuilder;
        }


    }
}
