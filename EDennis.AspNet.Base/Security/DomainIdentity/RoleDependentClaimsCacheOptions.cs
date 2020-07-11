using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class RoleDependentClaimsCacheOptions {
        public string Endpoint { get; set; }
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
