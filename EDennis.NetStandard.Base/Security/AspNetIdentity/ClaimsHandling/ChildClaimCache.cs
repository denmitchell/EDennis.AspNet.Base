using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Singleton cache used by IClaimsTransformation.
    /// This cache is an application/project-specific cache that is used to
    /// hold claims that are specific to the application.  This cache can be
    /// helpful in reducing size of identity tokens and/or for implementing
    /// role-like functionality without using ASP.NET Identity Roles. If
    /// desired, many highly granular child claims can be maintained in the
    /// cache without much impact on performance.
    /// </summary>
    public class ChildClaimCache : List<ChildClaim> {
        public ChildClaimCache(IOptionsMonitor<ChildClaimCache> settings) {
            //dynamically reload the cache when Configuration values change
            settings.OnChange(cache => { Clear(); AddRange(cache); });
        }
    }
}
