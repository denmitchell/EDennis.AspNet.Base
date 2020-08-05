using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetStandard.Base {
    public class ApiResourceOptions {
        public string Name { get; set; }
        public string[] Scopes { get; set; }
        public string[] UserClaims { get; set; }
    }
}
