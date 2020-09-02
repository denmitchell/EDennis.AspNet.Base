using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public interface IChildClaimCache {
        IEnumerable<ChildClaim> ChildClaims { get; }
    }
}
