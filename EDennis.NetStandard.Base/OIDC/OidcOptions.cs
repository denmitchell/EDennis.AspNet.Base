namespace EDennis.NetStandard.Base.Security {
    public class OidcOptions : ClientCredentialsOptions {
        public string ResponseType { get; set; }
        public string GrantType { get; set; }
        public string RedirectUri { get; set; }
        public string CodeChallengeMethod { get; set; } = "S256";
        public bool RequireHttpsMetadata { get; set; } = false;
        public bool SaveTokens { get; set; } = true;
        public bool ClearDefaultInboundClaimTypeMap { get; set; } = true;
        public bool AllowOfflineAccess { get; set; } = true;

    }
}
