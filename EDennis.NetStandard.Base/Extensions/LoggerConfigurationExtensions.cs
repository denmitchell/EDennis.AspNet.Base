using Microsoft.Extensions.Configuration;
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
}
