using System;

namespace EDennis.AspNet.Base.Security {
    public class UserClientClaims {
        public Guid UserId { get; set; }
        public string ClientId { get; set; }
        public string Claims { get; set; }
    }
}
