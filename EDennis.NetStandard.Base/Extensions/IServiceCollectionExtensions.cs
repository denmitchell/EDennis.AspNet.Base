using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.IO;

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
                .SetBasePath(Directory.GetCurrentDirectory())
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
