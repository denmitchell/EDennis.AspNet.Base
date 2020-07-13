using EDennis.AspNet.Base.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Middleware.MockUser {
    public class MockUserOptions {
        public bool Enabled { get; set; }
        public IEnumerable<ClaimModel> Claims {get; set;}
    }
}
