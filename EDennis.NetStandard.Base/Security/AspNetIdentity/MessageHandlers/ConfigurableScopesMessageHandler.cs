using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace EDennis.NetStandard.Base {
    public abstract class ConfigurableScopesMessageHandler : AuthorizationMessageHandler {
        public abstract string[] Scopes { get; }

        public ConfigurableScopesMessageHandler(IAccessTokenProvider provider, NavigationManager navigationManager) 
            : base(provider, navigationManager) {
            ConfigureHandler(
                       authorizedUrls: new[] { navigationManager.BaseUri },
                       scopes: Scopes
                       );
        }
    }
}
