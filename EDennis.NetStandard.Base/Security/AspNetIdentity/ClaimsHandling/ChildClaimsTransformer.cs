using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// The ChildClaimsTransformer class provides a mechanism for retrieving
    /// child claims from the singleton cache in any given app.  This can be
    /// helpful in reducing size of identity tokens and/or for implementing
    /// role-like functionality without using ASP.NET Identity Roles. If
    /// desired, many highly granular child claims can be maintained in the
    /// cache without much impact on performance.
    /// 
    /// The class is used just like any IClaimsTransformation implementation.  
    /// Configure the DI for IClaimsTransformation,ChildClaimsTransformer as 
    /// Scoped lifetime in Startup.cs
    /// </summary>
    public class ChildClaimsTransformer : IClaimsTransformation {

        private readonly IChildClaimCache _cache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache">Singleton holding cached ChildClaimCache</param>
        /// <param name="env"></param>
        /// <param name="encoder">Implementation of IAppClaimEncoder</param>
        public ChildClaimsTransformer(IChildClaimCache cache) {
            _cache = cache;
        }

        /// <summary>
        /// Adds any child claims defined in cache whose parent claims are in the claims principal
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) {
            var claims =
                await Task.Run(() =>
                {
                    return (from a in principal.Claims
                            join c in _cache.ChildClaims
                             on new { a.Type, a.Value } equals new { Type=c.ParentType, Value=c.ParentValue }
                         select new Claim(c.ChildType, c.ChildValue)
                        ).ToList();
                });

            if(claims != null && claims.Count() > 0)
                principal.AddIdentity(new ClaimsIdentity(claims));

            return principal;
        }
    }
}
