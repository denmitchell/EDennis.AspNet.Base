using System.Collections.Generic;
using System.Security.Claims;

namespace EDennis.AspNet.Base.Middleware {

    /// <summary>
    /// Scoped Lifetime
    /// </summary>
    public class ScopeProperties : Dictionary<string,string> {
        public Claim[] Claims { get; set; }
    }
}
