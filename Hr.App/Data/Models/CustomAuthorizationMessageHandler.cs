using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Hr.BlazorApp.Models {
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager)
            : base(provider, navigationManager) {
            ConfigureHandler(
                authorizedUrls: new[] { 
                    "https://localhost:5001",
                    "https://localhost:44327"
                },
                scopes: new[] { 
                    "Api1", "Api1.*.Get*", "Api1.*.Edit*", "Api1.*.Delete*",
                    "Api2", "Api2.*.Get*", "Api2.*.Edit*", "Api2.*.Delete*",
                });
        }


    }
}