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
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EDennis.Samples.ColorApp.Blazor {
    public class Program {
        public static async Task Main(string[] args) {

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            //builder.AddApiClients(typeof(RgbApiClient));
            builder.Services.TryAddTransient<RgbApiClient>();

            var apiClients = new ApiClients();
            builder.Configuration.Bind("ApiClients", apiClients);
            builder.Services.Configure<ApiClients>(builder.Configuration.GetSection("ApiClients"));


            builder.Services.AddTransient<ConfigurableAuthorizationMessageHandler<RgbApiClient>>();
            builder.Services.AddHttpClient(nameof(RgbApiClient), client => client.BaseAddress = new Uri("https://localhost:44341/"))
                .AddHttpMessageHandler<ConfigurableAuthorizationMessageHandler<RgbApiClient>>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddOidcAuthentication(options => {

                // see https://aka.ms/blazor-standalone-auth 
                builder.Configuration.Bind("Oidc:ProviderOptions", options.ProviderOptions);
                builder.Configuration.Bind("Oidc:UserOptions", options.UserOptions);

            });

            await builder.Build().RunAsync();

        }
    }
}
