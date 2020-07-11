using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class RoleIdClaimsTransformation : IClaimsTransformation {

        private readonly RoleDependentClaimsCache _cache;

        public RoleIdClaimsTransformation(RoleDependentClaimsCache cache ) {
            _cache = cache;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) {
            await Task.Run(() => {
                var newClaims = new List<Claim>();
                foreach (var roleId in principal.Claims
                        .Where(c => c.Type == "role_id")
                        .Select(c => Guid.Parse(c.Value))) {
                    if (_cache.TryGetValue(roleId, out IEnumerable<Claim> claims))
                        newClaims.AddRange(claims);
                }
                principal.AddIdentity(new ClaimsIdentity(newClaims));
            });
            return principal;
        }
    }
}
