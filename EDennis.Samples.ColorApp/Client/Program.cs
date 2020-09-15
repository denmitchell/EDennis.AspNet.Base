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


            OidcProviderOptions oidcOptions = new OidcProviderOptions();


            /*
            builder.Services.AddOidcAuthentication(options => {
                builder.Configuration.Bind("Security:OidcProviderOptions", options.ProviderOptions);
                oidcOptions = options.ProviderOptions;
                options.UserOptions.NameClaim = JwtClaimTypes.Name;
                options.UserOptions.RoleClaim = options.ProviderOptions.DefaultScopes.FirstOrDefault(s => s.StartsWith("role:"));
            })
            .AddAccountClaimsPrincipalFactory<DomainAccountClaimsPrincipalFactory<RemoteUserAccount>>();
            */

           builder.Services.AddHttpClient(clientName, 
                client => client.BaseAddress = new Uri(baseAddress))
                .AddHttpMessageHandler(sp=> {
                    var handler = sp.GetService<BaseAddressAuthorizationMessageHandler>()
                        .ConfigureHandler(
                            authorizedUrls: new string[] { baseAddress },
                            scopes: new string[] {  
                            "openid",
                            "profile",
                            "role:EDennis.Samples.ColorApp.Server"
                            }
                        );
                    return handler;
                });


            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient(clientName));

            builder.Services.AddScoped<RgbApiClient>();

            builder.Services.AddOidcAuthentication(options => {
                //options.ProviderOptions.Authority = "https://localhost:5001/";
                // Configure your authentication provider options here.
                // For more information, see https://aka.ms/blazor-standalone-auth
                builder.Configuration.Bind("Security:OidcProviderOptions", options.ProviderOptions);
            });
            /*
            builder.Services.AddApiAuthorization(configure=> {
                configure.UserOptions.RoleClaim = "role:EDennis.Samples.ColorApp.Server";
                configure.UserOptions.NameClaim = "name";
                });
            */
            await builder.Build().RunAsync();
        }
    }
}
