using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public class ApiResourceEditModel {
        public string Name { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public IEnumerable<string> UserClaimTypes { get; set; }
    }
}
