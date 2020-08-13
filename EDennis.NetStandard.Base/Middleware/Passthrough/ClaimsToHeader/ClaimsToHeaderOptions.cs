using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class ClaimsToHeaderOptions : HeaderToClaimsOptions {
        public List<string> ClaimTypes { get; set; }

    }
}
