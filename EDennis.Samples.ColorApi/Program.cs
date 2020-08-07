using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using EDennis.NetStandard.Base;
using EDennis.NetStandard.Base.Security.AspNetIdentity.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.UriParser;
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

            if (args.Contains("/idp-config")) {
                Log.Information("Generating IDP Config file...");
                ConfigStubGenerator.GenerateIdpConfigStub<Startup>(5000, 44341, IdpConfigType.ClientCredentials);
                Log.Information("Exiting...");
                return;
            }


            CreateHostBuilder(args).Build().Run();
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
