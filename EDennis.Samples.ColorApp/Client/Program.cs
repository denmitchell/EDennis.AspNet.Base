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


           builder.Services.AddHttpClient(clientName, 
                client => client.BaseAddress = new Uri(baseAddress))
                .AddHttpMessageHandler(sp=> {
                    var handler = sp.GetService<AuthorizationMessageHandler>()
                        .ConfigureHandler(
                            authorizedUrls: new string[] { baseAddress },
                            scopes: new string[] {  
                            "openid",
                            "profile",
                            "EDennis.Samples.ColorApp.ServerAPI"
                            }
                        );
                    return handler;
                });


            builder.Services.AddScopedRequestMessage(builder.Configuration);

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(clientName));

            builder.Services.AddScoped<RgbApiClient>();

            builder.Services.AddApiAuthorization(configure=> {
                configure.UserOptions.RoleClaim = "role:EDennis.Samples.ColorApp.Server";
                configure.UserOptions.NameClaim = "name";
                configure.ProviderOptions.ConfigurationEndpoint = "_configuration/EDennis.Samples.ColorApp.Client";
                });

            await builder.Build().RunAsync();
        }
    }
}
