using IdentityModel;
namespace EDennis.NetStandard.Base {
    public class ClientCredentialsOptions {
        public string Authority { get; set; } //needed for token generation and validation
        //public string Audience { get; set; } //not used when using ApiScopes in IdentityServer
        public string ClientId { get; set; } //needed for token generation
        public string ClientSecret { get; set; } //needed for token generation
        public string[] Scopes { get; set; } //needed for token generation

    }

}
