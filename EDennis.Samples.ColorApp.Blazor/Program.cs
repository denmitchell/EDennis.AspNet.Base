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
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;

namespace EDennis.Samples.ColorApp.Blazor {
    public class Program {
        public static async Task Main(string[] args) {

            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            //builder.AddApiClients(typeof(RgbApiClient));
            builder.Services.TryAddTransient<RgbApiClient>();
            builder.Services.TryAddTransient<WeatherApiClient>();

            var apiClients = new ApiClients();
            builder.Configuration.Bind("ApiClients", apiClients);
            builder.Services.Configure<ApiClients>(builder.Configuration.GetSection("ApiClients"));


            builder.Services.AddTransient<ConfigurableAuthorizationMessageHandler<RgbApiClient>>();
            builder.Services.AddTransient<WeatherApiAuthorizationMessageHandler>();

            builder.Services.AddHttpClient<RgbApiClient>(client => client.BaseAddress = new Uri("https://localhost:44341/"))
                .AddHttpMessageHandler<ConfigurableAuthorizationMessageHandler<RgbApiClient>>();
            //builder.Services.AddHttpClient(nameof(RgbApiClient), client => client.BaseAddress = new Uri("https://localhost:44341/"))
            //    .AddHttpMessageHandler<ConfigurableAuthorizationMessageHandler<RgbApiClient>>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddOidcAuthentication(options => {

                // see https://aka.ms/blazor-standalone-auth 
                builder.Configuration.Bind("Oidc:ProviderOptions", options.ProviderOptions);
                builder.Configuration.Bind("Oidc:UserOptions", options.UserOptions);

            });

            builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new Uri("https://localhost:44305/"))
                .AddHttpMessageHandler<WeatherApiAuthorizationMessageHandler>();

            await builder.Build().RunAsync();

        }
    }

    public class WeatherApiAuthorizationMessageHandler : AuthorizationMessageHandler {

        public WeatherApiAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager) : base(provider, navigationManager) {
            ConfigureHandler(
                authorizedUrls: new[] { "https://localhost:44305" }
                );
        }
    }
}
