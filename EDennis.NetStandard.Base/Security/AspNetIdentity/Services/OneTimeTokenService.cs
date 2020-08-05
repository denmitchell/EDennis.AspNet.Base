using IdentityModel.Client;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;


namespace EDennis.NetStandard.Base {
    public class OneTimeTokenService : ITokenService {

        public ClientCredentialsOptions Options { get; }


        public OneTimeTokenService(ClientCredentialsOptions options) {
            Options = options;
        }


        public async Task AssignTokenAsync(HttpClient client) {

            var idpClient = new HttpClient();

            // get metadata
            var disco = await idpClient.GetDiscoveryDocumentAsync(Options.Authority);
            if (disco.IsError) {
                Debug.WriteLine(disco.Error);
                throw new Exception($"Could not retrieve Discovery Document from IDP at {Options.Authority}");
            }

            // get token
            var tokenResponse = await idpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                Address = disco.TokenEndpoint,

                ClientId = Options.ClientId,
                ClientSecret = Options.ClientSecret,
                Scope = string.Join(" ", Options.Scopes)
            });

            if (tokenResponse.IsError) {
                Debug.WriteLine(tokenResponse.Error);
                throw new Exception($"Token Error from IDP: {tokenResponse.ErrorDescription}");
            }

            var token = tokenResponse.AccessToken;

            client.SetBearerToken(token);
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string token) {
            await Task.Run(() => { });
            throw new NotImplementedException("OneTimeTokenService does not implement ValidateTokenAsync");
        }


    }


}
