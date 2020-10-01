using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;


namespace EDennis.NetStandard.Base {
    public class BlazorAuthenticationStateProvider : AuthenticationStateProvider {

        public BlazorAuthenticationStateProvider(IJSRuntime jsRuntime) {
            _jsRuntime = jsRuntime;
        }

        private readonly IJSRuntime _jsRuntime;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {

            var claimsJson = await _jsRuntime.InvokeAsync<string>("EDennisApplication.getCookie", ClaimsCookieMiddleware.COOKIE_KEY);

            try {
                var claims = JsonSerializer
                    .Deserialize<ClaimView[]>(claimsJson)
                    .Select(c=> new Claim(c.Type,c.Value))
                    .ToArray();

                var identity = new ClaimsIdentity(claims, "MyClaims");
                var user = new ClaimsPrincipal(identity);

                return new AuthenticationState(user);
            } catch (JsonException ex) {
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }
    }
}
