using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    /// <summary>
    /// Singleton cache used by IClaimsTransformation
    /// </summary>
    public class DomainRoleClaimCache : List<RoleClaimView> {
        public DomainRoleClaimCache(IOptionsMonitor<DomainRoleClaimCache> settings) {
            settings.OnChange(cache => { Clear(); AddRange(cache); });
        }
    }
}
