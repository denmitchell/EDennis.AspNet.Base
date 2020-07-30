using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.Security {

    /// <summary>
    /// With the DomainIdentity classes, DomainRoleClaims (UserRoleClaims<Guid>) was
    /// purposely omitted from the model in order to make role-associated claims a
    /// concern of individual applications, where these claims are cached in a 
    /// singleton (DomainRoleClaimCache).
    /// The DomainRolesClaimsTransformer class provides a mechanism for retrieving
    /// role-associated claims from the singleton cache.  The class is used just
    /// like any IClaimsTransformation implementation.  Configure the DI for 
    /// IClaimsTransformation,DomainRolesClaimsTransformer as Scoped lifetime 
    /// in Startup.cs
    /// </summary>
    public class DomainRoleClaimsTransformer : IClaimsTransformation {

        private readonly DomainRoleClaimCache _cache;
        private readonly IHostEnvironment _env;

        public DomainRoleClaimsTransformer(DomainRoleClaimCache cache, IHostEnvironment env) {
            _cache = cache;
            _env = env;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) {
            var claims =
                await Task.Run(() =>
                {
                    return
                    (from a in principal.Claims.Where(c => c.Type == _env.ApplicationName)
                     join c in _cache
                         on a.Value equals c.RoleName
                     select new Claim(c.ClaimType, c.ClaimValue)
                    ).ToList();
                });

            principal.AddIdentity(new ClaimsIdentity(claims));

            return principal;
        }
    }
}
