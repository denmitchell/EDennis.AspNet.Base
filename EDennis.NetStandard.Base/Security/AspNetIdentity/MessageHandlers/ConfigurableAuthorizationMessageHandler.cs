using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;
using System.Linq;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Defines an Authorization message handler that configures 
    /// itself based upon AuthorizationMessageHandlerOptions found in configuration.
    /// </summary>
    public class ConfigurableAuthorizationMessageHandler : AuthorizationMessageHandler {
        public ConfigurableAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigationManager,
            IOptionsMonitor<AuthorizationMessageHandlerOptions> options)
            : base(provider, navigationManager) {

            //System.Diagnostics.Debug.WriteLine(provider.GetType().FullName);

            ConfigureHandler(
                       authorizedUrls: options.CurrentValue.AuthorizedUrls,
                       scopes: options.CurrentValue.Scopes
          );              
                       
        }
    }
}
