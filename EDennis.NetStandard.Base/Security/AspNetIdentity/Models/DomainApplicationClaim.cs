using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class DomainApplicationClaim {
        public string Application { get; set; }
        public string ClaimTypePrefix { get; set; }
        public string ClaimValue { get; set; }
        public bool OrgAdminable { get; set; }
    }
}
