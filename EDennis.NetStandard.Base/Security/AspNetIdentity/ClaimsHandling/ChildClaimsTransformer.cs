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

        private readonly ChildClaimCache _cache;
        private readonly IHostEnvironment _env;
        private readonly IAppClaimEncoder _encoder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache">Singleton holding cached ChildClaimCache</param>
        /// <param name="env"></param>
        /// <param name="encoder">Implementation of IAppClaimEncoder</param>
        public ChildClaimsTransformer(ChildClaimCache cache, IHostEnvironment env,
            IAppClaimEncoder encoder) {
            _cache = cache;
            _env = env;
            _encoder = encoder;
        }

        /// <summary>
        /// Filters the list of incoming claims by application name and also adds any
        /// derived claims
        /// 
        /// Assumes that incoming claims from the user/client are encoded according to the
        /// IAppClaimEncoder implementation.  If using the DefaultAppClaimEncoder, then
        /// claim value is prefixed by application name and a colon.  
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) {
            var claims =
                await Task.Run(() =>
                {
                var appClaims = principal.Claims
                    .Select(c => _encoder.Decode(c))
                    .Where(c => c.Application == _env.ApplicationName)
                    .Select(c => new Claim(c.ClaimType, c.ClaimValue))
                    .AsQueryable();

                    return (from a in appClaims
                         join c in _cache
                             on new { a.Type, a.Value } equals new { Type=c.ParentType, Value=c.ParentValue }
                         select new Claim(c.ClaimType, c.ClaimValue)
                        ).ToList();
                });

            principal.AddIdentity(new ClaimsIdentity(claims));

            return principal;
        }
    }
}
