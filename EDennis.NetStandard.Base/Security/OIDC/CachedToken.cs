using IdentityModel.Client;
using System;

namespace EDennis.NetStandard.Base {
    public class CachedToken {
        public TokenResponse TokenResponse { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
