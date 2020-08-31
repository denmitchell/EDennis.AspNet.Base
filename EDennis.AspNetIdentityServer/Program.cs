using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Linq;
using System.Text.RegularExpressions;


namespace EDennis.AspNetIdentityServer {

    public class Program {

        public const string CONFIGS_DIR = "Configs";
        public const string DBVIEW_DIR = "Logs";
        public const string DBVIEW_FILE = "DbView.json";
        public static Regex FILE_PROJECT_EXTRACTOR = new Regex(@"(?<=Configs\\)([A-Za-z0-9_.]+)(?=\.json)");
        public const string IDENTITY_RESOURCES_FILE = "IdentityResources.json";


        public static void Main(string[] args) {

            Log.Logger = new LoggerConfiguration()
                .GetLoggerFromConfiguration<Program>("Logging:Serilog");

            try {
                Log.Information("Starting host...");
                var host = CreateHostBuilder(args).Build();

                var configLoader = new SeedDataLoader(host, args, Log.Logger);
                configLoader.Load();

                if(!args.Contains("/norun"))
                    host.Run();

            } catch (Exception ex) {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }


        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>                
                {
                    webBuilder.UseSerilog();
                    webBuilder.UseStartup<Startup>();
                });
    }

}
