using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetStandard.Base {
    public class ScopedRequestMessageOptions {
            public string[] HeadersToCapture { get; set; } = new string[] { CachedTransactionOptions.COOKIE_KEY, HeaderToClaimsOptions.HEADER_KEY };
            public string[] CookiesToCapture { get; set; } = new string[] { CachedTransactionOptions.COOKIE_KEY };
        }
}
