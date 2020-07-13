namespace EDennis.AspNet.Base {
    public class OidcOptions {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ResponseType { get; set; }
        public string GrantType { get; set; }
        public string[] Scope { get; set; }

        public string AutoLoginUsername { get; set; }
        public string AutoLoginPassword { get; set; }
        public string RedirectUri { get; set; }

        public string CodeChallengeMethod { get; set; } = "S256";

        public bool RequireHttpsMetadata { get; set; } = false;
        public bool SaveTokens { get; set; } = true;
        public bool ClearDefaultInboundClaimTypeMap { get; set; } = true;

        public string UserScopePrefix { get; set; } = "user_";
        public string ExclusionPrefix { get; set; } = "-";

    }
}
