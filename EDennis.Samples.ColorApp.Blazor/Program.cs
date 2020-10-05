using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EDennis.NetStandard.Base;
using EDennis.NetStandard.Base.Extensions;

namespace EDennis.Samples.ColorApp.Blazor {
    public class Program {
        public static async Task Main(string[] args) {

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.AddApiClients(typeof(RgbApiClient));

            await builder.Build().RunAsync();
        }
    }
}
