using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Client {
    public class Program {
        public static async Task Main(string[] args) {


            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var baseAddress = builder.HostEnvironment.BaseAddress;

            builder.Services.AddHttpClient<RgbApiClient>(
                     client => client.BaseAddress = new Uri(baseAddress));

            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, BlazorAuthenticationStateProvider>();

            await builder.Build().RunAsync();
        }
    }
}
