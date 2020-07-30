namespace EDennis.NetStandard.Base.Security {
    public class ClientCredentialsOptions {
        public string Authority { get; set; }
        public string Audience { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scope { get; set; }
    }
}
