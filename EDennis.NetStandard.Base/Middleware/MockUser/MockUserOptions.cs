using EDennis.NetStandard.Base.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.Middleware.MockUser {
    public class MockUserOptions {
        public bool Enabled { get; set; }
        public Dictionary<string,string[]> Claims {get; set;}
    }
}
