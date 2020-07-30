using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class ClientCredentialsTokenService {

        private readonly ClientCredentialsOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ClientCredentialsTokenService> _logger;
        private string _token;
        private DateTime _expiresOn;

        public const int EXPIRATION_BUFFER_IN_SECONDS = 60;

        public ClientCredentialsTokenService(IOptionsMonitor<ClientCredentialsOptions> options,
            IHttpClientFactory httpClientFactory, ILogger<ClientCredentialsTokenService> logger) {
            _options = options.CurrentValue;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task AssignTokenAsync(HttpClient client) {

            if (DateTime.Now.AddSeconds(EXPIRATION_BUFFER_IN_SECONDS) >= _expiresOn)
                await UpdateCachedTokenAsync();

            client.SetBearerToken(_token);
        }

        public async Task UpdateCachedTokenAsync() {

            // get metadata
            var idpClient = _httpClientFactory.CreateClient("ClientCredentialsTokenService");
            var disco = await idpClient.GetDiscoveryDocumentAsync(_options.Authority);
            if (disco.IsError) {
                _logger.LogError(disco.Error);
                return;
            }

            // get token
            var tokenResponse = await idpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                Address = disco.TokenEndpoint,

                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Scope = string.Join(' ', _options.Scope)
            });

            if (tokenResponse.IsError) {
                _logger.LogError(disco.Error);
                return;
            }

            _token = tokenResponse.AccessToken;
            _expiresOn = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);

        }
    }

}
