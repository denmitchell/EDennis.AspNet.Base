using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;

namespace EDennis.Samples.ColorApi {
    public class Program {
        public static void Main(string[] args) {

            Log.Logger = new LoggerConfiguration()
                .GetLoggerFromConfiguration<Program>("Logging:Serilog");

            Log.Information("Starting host ...");


            if (args.Contains("/idp-config")) {
                Log.Information("Generating IDP Config file...");
                SeedDataGenerator.GenerateIdpConfigStub<Startup>(5000, 44341, false);
                Log.Information("Exiting...");
                return;
            }


            CreateHostBuilder(args).Build().Run();
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseSerilog();
                });
    }
}
