using System;
using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public class ClientEditModel {
        public string ClientId { get; set; }
        public Dictionary<string, DateTime?> ClientSecrets { get; set; } 
            = new Dictionary<string, DateTime?> { { "secret", DateTime.Now.AddDays(5000) } }; 
        public IEnumerable<string> AllowedScopes { get; set; }
        public IEnumerable<string> AllowedGrantTypes { get; set; } = new string[] { "code" };
        public bool RequireConsent { get; set; } = false;
        public bool RequirePkce { get; set; } = true;
        public bool AllowOfflineAccess { get; set; } = true;
        public string ClientClaimsPrefix { get; set; } = "";
        public IEnumerable<string> AllowedCorsOrigins { get; set; }
        public IEnumerable<string> RedirectUris { get; set; }
        public IEnumerable<string> PostLogoutRedirectUris { get; set; }

    }
}
