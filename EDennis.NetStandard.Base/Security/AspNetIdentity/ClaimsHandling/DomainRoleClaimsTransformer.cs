using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

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
        private readonly IAppClaimEncoder _encoder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache">Singleton holding cached RoleClaimView</param>
        /// <param name="env"></param>
        /// <param name="encoder">Implementation of IAppClaimEncoder</param>
        public DomainRoleClaimsTransformer(DomainRoleClaimCache cache, IHostEnvironment env,
            IAppClaimEncoder encoder) {
            _cache = cache;
            _env = env;
            _encoder = encoder;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) {
            var claims =
                await Task.Run(() =>
                {
                    return
                    (from a in principal.Claims
                        .Select(c => _encoder.Decode(c))
                        .Where(c => c.ClaimType == JwtClaimTypes.Role 
                            && c.Application == _env.ApplicationName)
                        .Select(c=> new { c.ClaimType, c.ClaimValue})
                     join c in _cache
                         on a.ClaimValue equals c.RoleName
                     select new Claim(c.ClaimType, c.ClaimValue)
                    ).ToList();
                });

            principal.AddIdentity(new ClaimsIdentity(claims));

            return principal;
        }
    }
}
