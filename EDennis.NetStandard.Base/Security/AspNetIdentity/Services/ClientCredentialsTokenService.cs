using IdentityModel.Client;
using JWT.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;

namespace EDennis.NetStandard.Base {
    public class ClientCredentialsTokenService : ITokenService {

        private readonly ClientCredentialsOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ClientCredentialsTokenService> _logger;
        private string _token;
        private DateTime _expiresOn;
        private JsonWebKey _jwk;
        private SigningCredentials _signingCredentials { get; set; }
        private DateTime _jwkLastRefreshed;


        public const int EXPIRATION_BUFFER_IN_SECONDS = 60;

        public object lockObj = new object();

        public ClientCredentialsTokenService(IOptionsMonitor<ClientCredentialsOptions> options,
            IHttpClientFactory httpClientFactory, ILogger<ClientCredentialsTokenService> logger) {
            _options = options.CurrentValue;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            Task.Run(() => GetJsonWebKeyAsync());
        }



        private async Task GetJsonWebKeyAsync() {
            var idpClient = GetIdpClient();
            var disco = await GetDiscoveryDocumentAsync(idpClient);
            var response = await idpClient.GetJsonWebKeySetAsync(disco.JwksUri);
            if (response.IsError) {
                _logger.LogError(disco.Error);
            } else {
                var key = response.KeySet.Keys.FirstOrDefault(a => a.Use == "sig");
                _jwk = new JsonWebKey(JsonSerializer.Serialize(key));

                if (_jwk.Kty == "RSA")
                    _signingCredentials = new SigningCredentials(_jwk, SecurityAlgorithms.RsaSha256Signature);
                else if (_jwk.Kty == "oct")
                    _signingCredentials = new SigningCredentials(_jwk, SecurityAlgorithms.HmacSha256Signature);

                _jwkLastRefreshed = DateTime.Now;
            }
        }



        public async Task AssignTokenAsync(HttpClient client) {

            if (DateTime.Now.AddSeconds(EXPIRATION_BUFFER_IN_SECONDS) >= _expiresOn) {
                var idpClient = GetIdpClient();
                var disco = await GetDiscoveryDocumentAsync(idpClient);
                await UpdateCachedTokenAsync(idpClient, disco);
            }

            client.SetBearerToken(_token);
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string token) {
            await Task.Run(() => { });
            var tokenHandler = new JwtSecurityTokenHandler();
            try {
                var cp = tokenHandler.ValidateToken(token,
                    new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = _options.Authority,
                        ValidateAudience = true,
                        ValidAudience = _options.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = _jwk,
                    }, out SecurityToken _);

                return cp;

            } catch (SecurityTokenDecryptionFailedException ex) {
                _logger.LogError(ex, ex.Message);
                lock (lockObj) {
                    if (_jwkLastRefreshed.AddHours(12) > DateTime.Now)
                        GetJsonWebKeyAsync().Wait();
                    else if (_jwkLastRefreshed.AddSeconds(60) < DateTime.Now)
                        throw ex;
                }
            } catch (Exception ex) {
                _logger.LogError(ex, ex.Message);
            }

            return null;
        }


        private HttpClient GetIdpClient() {
            return _httpClientFactory.CreateClient("ClientCredentialsTokenService");
        }

        private async Task<DiscoveryDocumentResponse> GetDiscoveryDocumentAsync(HttpClient idpClient) {
            // get metadata
            var disco = await idpClient.GetDiscoveryDocumentAsync(_options.Authority);
            if (disco.IsError) {
                _logger.LogError(disco.Error);
                return null;
            }
            return disco;
        }

        private async Task UpdateCachedTokenAsync(HttpClient idpClient, DiscoveryDocumentResponse disco) {

            // get token
            var tokenResponse = await idpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                Address = disco.TokenEndpoint,

                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Scope = string.Join(" ", _options.Scope)
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
