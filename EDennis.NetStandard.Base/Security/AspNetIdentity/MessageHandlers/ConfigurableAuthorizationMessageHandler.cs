using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Defines an Authorization message handler that configures 
    /// itself based upon AuthorizationMessageHandlerOptions found in configuration.
    /// </summary>
    public class ConfigurableAuthorizationMessageHandler<TApiClient> : AuthorizationMessageHandler {
        public ConfigurableAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigationManager,
            IOptionsMonitor<ApiClients> apiClients)
            : base(provider, navigationManager) {

            if (!apiClients.CurrentValue.TryGetValue(typeof(TApiClient).Name, out ApiClient apiClient))
                throw new ArgumentException($"{typeof(TApiClient).Name} is not found in ApiClients section of configuration");

            var authorizedUrls = new string[] { apiClient.TargetUrl };
            if (apiClient.OtherAuthorizedUrls != null)
                authorizedUrls = authorizedUrls.Union(apiClient.OtherAuthorizedUrls).ToArray();

            ConfigureHandler(
                       authorizedUrls: authorizedUrls,
                       scopes: apiClient.Scopes
          );              
                       
        }
    }
}
