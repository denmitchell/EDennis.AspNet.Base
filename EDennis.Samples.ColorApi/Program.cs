using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EDennis.Samples.ColorApi {
    public class Program {
        public static void Main(string[] args) {
            Log.Logger = new LoggerConfiguration()
                       .Enrich.FromLogContext()
                       .WriteTo.Console()
                       .WriteTo.Seq(
                            Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
                       .CreateLogger();

            if(args.Contains("/init-idp"))
                InitIdp();

            CreateHostBuilder(args).Build().Run();
        }

        private static void InitIdp() {
            var env = Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");
            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true)
                .AddJsonFile($"appsettings.{env}.json", true)
                .Build();

            var options = new ClientCredentialsOptions();
            config.GetSection("Security:ClientCredentials").Bind(options);
            var tokenService = new OneTimeTokenService(options);

            var client = new AppInitControllerClient<Startup>(config, tokenService);

            client.LoadApi("Security:ApiResource");

            //Not needed for Child API, unless used for MockClient
            //client.LoadClientFromClientCredentialsOptions("Security:ClientCredentials");

            //Not needed for Child API
            //client.LoadTestUsers("Security:TestUsers");
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
