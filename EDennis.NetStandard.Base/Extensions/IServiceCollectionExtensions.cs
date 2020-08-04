using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace EDennis.NetStandard.Base.Extensions {
    
    
    public static class IServiceCollectionExtensions {

        /// <summary>
        /// Configures the TokenService and the HttpClient for all ProxyQueryControllers
        /// and ProxyCrudControllers.
        /// Note: This extension method requires the existence of a top-level configuration
        /// section called "ProxyClients" that binds to a Dictionary<string,ProxyClient>
        /// and whose key is the name of the Proxy Controller, minus the word Controller.
        /// </summary>
        public static IServiceCollection AddProxyClients<TTokenService>(this IServiceCollection services,
            IConfiguration config)
            where TTokenService : class, ITokenService {

            services.TryAddSingleton<ITokenService, TTokenService>();

            var clients = new ProxyClients();
            config.GetSection("ProxyClients").Bind(clients);

            foreach(var client in clients) {
                var clientName = client.Key;
                services.AddHttpClient(clientName, options => {
                    options.BaseAddress = new Uri(client.Value.TargetUrl);
                });
            }

            return services;
        }
    }
}
