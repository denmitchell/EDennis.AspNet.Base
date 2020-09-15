using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;


namespace EDennis.NetStandard.Base {

    /// <summary>
    /// for Blazor
    /// From: https://github.com/cradle77/BlazorSecurityDemo/blob/master/BlazorClient/Security/ArrayClaimsPrincipalFactory.cs
    /// </summary>
    /// <typeparam name="TAccount"></typeparam>
    public class DomainAccountClaimsPrincipalFactory<TAccount> : AccountClaimsPrincipalFactory<TAccount> where TAccount : RemoteUserAccount {
        public DomainAccountClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
        : base(accessor) { }


        // when a user belongs to multiple roles, IS4 returns a single claim with a serialised array of values
        // this class improves the original factory by deserializing the claims in the correct way
        public async override ValueTask<ClaimsPrincipal> CreateUserAsync(TAccount account, RemoteAuthenticationUserOptions options) {
            var user = await base.CreateUserAsync(account, options);

            var identity = (ClaimsIdentity)user.Identity;

            if (account != null) {
                foreach (var kvp in account.AdditionalProperties) {
                    var name = kvp.Key;
                    var value = kvp.Value;
                    if (value != null &&
                        (value is JsonElement element && element.ValueKind == JsonValueKind.Array)) {
                        identity.RemoveClaim(identity.FindFirst(kvp.Key));

                        var claims = element.EnumerateArray()
                            .Select(x => new Claim(kvp.Key, x.ToString()));

                        identity.AddClaims(claims);
                    }
                }
            }

            return user;
        }
    }
}
