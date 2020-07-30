using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace EDennis.NetStandard.Base.Security {
    /// <summary>
    /// Singleton cache used by IClaimsTransformation
    /// </summary>
    public class DomainRoleClaimCache : List<DomainRoleClaim> {
        public DomainRoleClaimCache(IOptionsMonitor<DomainRoleClaimCache> settings) {
            settings.OnChange(cache => { Clear(); AddRange(cache); });
        }
    }
}
