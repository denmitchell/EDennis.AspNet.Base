using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace EDennis.Samples.ColorApp.Client {
    public class ColorAppMessageHandler : DomainIdentityMessageHandler {
        public ColorAppMessageHandler(IAccessTokenProvider provider, 
            NavigationManager navigationManager) : base(provider, navigationManager) {
        }

        public override string TargetApplicationName => "EDennis.Samples.ColorApp.Server";
    }
}
