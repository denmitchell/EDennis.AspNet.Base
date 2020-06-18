namespace EDennis.AspNet.Base.Middleware {
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
    }
}
