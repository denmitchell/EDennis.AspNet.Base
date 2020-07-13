using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public class IdentityResourceEditModel {
        public string Name { get; set; }
        public IEnumerable<string> UserClaimTypes { get; set; }
    }
}
