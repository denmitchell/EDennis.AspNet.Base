using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class BlazorAuthenticationStateProvider : AuthenticationStateProvider {

        public BlazorAuthenticationStateProvider(IJSRuntime jsRuntime) {
            _jsRuntime = jsRuntime;
        }

        private readonly IJSRuntime _jsRuntime;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync() {

            var claimsJson = await _jsRuntime.InvokeAsync<string>("EDennisApplication.getCookie", ClaimsCookieMiddleware.COOKIE_KEY);
            var claims = JsonSerializer.Deserialize<Claim[]>(claimsJson);

            var identity = new ClaimsIdentity(claims,"MyClaims");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
    }
}
