using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Middleware {
    public class TransactionScopeMiddlewareOptions {
        public Dictionary<string, string[]> EnabledForClaims { get; set; }

    }
}
