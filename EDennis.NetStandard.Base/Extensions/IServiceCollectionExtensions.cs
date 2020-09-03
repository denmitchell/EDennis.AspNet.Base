using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace EDennis.NetStandard.Base {


    public static class LoggerConfigurationExtensions {

        public static Serilog.Core.Logger GetLoggerFromConfiguration<TProgram>(
            this LoggerConfiguration loggerConfiguration, IConfiguration config, string configKey = "Logging:Serilog") {

            var slogger = loggerConfiguration
                .ReadFrom.Configuration(config, configKey)
                .CreateLogger();

            return slogger;
        }

        public static Serilog.Core.Logger GetLoggerFromConfiguration<TProgram>(
            this LoggerConfiguration loggerConfiguration, string configKey = "Logging:Serilog") {

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env}.json", true, true)
                .Build();

            return GetLoggerFromConfiguration<TProgram>(loggerConfiguration, config, configKey);
        }


    }

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
                IConfiguration config, string clientName = null, string configKeyParent = API_CLIENTS_CONFIG_KEY_PARENT)
            where TClientInterface : class
            where TClientImplementation : class, TClientInterface {

            var builder = new TypedClientBuilder { ConfigKeyParent = configKeyParent, Configuration = config, Services = services };
            return builder.AddApiClient<TClientInterface, TClientImplementation>(clientName);
        }

    }
}
