using EDennis.NetStandard.Base;
using IdentityModel;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Client {
    public class Program {
        public static async Task Main(string[] args) {


            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var baseAddress = builder.HostEnvironment.BaseAddress;
            var clientName = typeof(Program).Namespace;

            builder.Services.Configure<AuthorizationMessageHandlerOptions>(
                builder.Configuration.GetSection("AuthorizationMessageHandler"));

            builder.Services.AddScoped<ConfigurableAuthorizationMessageHandler>();

            builder.Services.AddHttpClient<RgbApiClient>(
                     client => client.BaseAddress = new Uri(baseAddress))
                 .AddHttpMessageHandler<ConfigurableAuthorizationMessageHandler>();


            builder.Services.AddApiAuthorization(configure => {
                configure.UserOptions.RoleClaim = "role:EDennis.Samples.ColorApp.Server";
                configure.UserOptions.NameClaim = "name";
                configure.ProviderOptions.ConfigurationEndpoint = "_configuration/EDennis.Samples.ColorApp.Client";
            });

            await builder.Build().RunAsync();
        }
    }
}
