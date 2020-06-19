using Hr.BlazorApp.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hr.BlazorApp {
    public class Program {
        public static async Task Main(string[] args) {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            var apis = new Dictionary<string, string>();
            builder.Configuration.Bind("Apis", apis);

            var oidcProviderOptions = new OidcProviderOptions();
            builder.Configuration.GetSection("OidcProvider").Bind(oidcProviderOptions);

            builder.Services.AddTransient<CustomAuthorizationMessageHandler>();


            foreach (var api in apis) {
                builder.Services.AddHttpClient(api.Key, client => {
                    client.BaseAddress = new Uri(api.Value);
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
                }).AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
            }


            builder.Services.AddOidcAuthentication(options => {
                // see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("OidcProvider", options.ProviderOptions);
            });

            await builder.Build().RunAsync();
        }
    }
}
