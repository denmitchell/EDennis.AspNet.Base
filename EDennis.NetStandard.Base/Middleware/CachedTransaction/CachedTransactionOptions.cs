using System.Collections.Generic;

namespace EDennis.NetStandard.Base.Middleware {
    public class CachedTransactionOptions {
        public Dictionary<string, string[]> EnabledForClaims { get; set; }

        public const string COOKIE_KEY = "CachedTransaction";
        public const string ROLLBACK_PATH = "/rollback-cached-transaction";
        public const string COMMIT_PATH = "/commit-cached-transaction";

    }
}
