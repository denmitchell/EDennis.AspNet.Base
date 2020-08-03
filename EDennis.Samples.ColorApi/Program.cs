using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
