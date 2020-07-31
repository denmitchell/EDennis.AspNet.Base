using System.Collections.Generic;
using System.Dynamic;

namespace EDennis.NetStandard.Base.Web {
    public class PassthroughClaimsOptions {
        public bool Enabled { get; set; }

        public const string CLAIMS_HEADER = "X-PassThroughClaims";
        public List<string> ClaimTypes { get; set; }

    }
}
