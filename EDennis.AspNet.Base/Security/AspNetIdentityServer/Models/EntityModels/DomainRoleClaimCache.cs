using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    /// <summary>
    /// Singleton cache used by IClaimsTransformation
    /// </summary>
    public class DomainRoleClaimCache : List<DomainRoleClaim> {
    }
}
