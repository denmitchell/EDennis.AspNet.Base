using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class ClientCredentialsTokenService : ITokenService {

        public ClientCredentialsOptions Options { get; }
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ClientCredentialsTokenService> _logger;
        private readonly ConcurrentDictionary<string,CachedToken> _tokenCache 
            = new ConcurrentDictionary<string, CachedToken>();
        private JsonWebKey _jwk;
        private DateTime _jwkLastRefreshed;


        public const int EXPIRATION_BUFFER_IN_SECONDS = 60;

        public object lockObj = new object();

        public ClientCredentialsTokenService(IOptionsMonitor<ClientCredentialsOptions> options,
            IHttpClientFactory httpClientFactory, ILogger<ClientCredentialsTokenService> logger) {
            Options = options.CurrentValue;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            Task.Run(() => GetJsonWebKeyAsync()).Wait();
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
                _jwkLastRefreshed = DateTime.Now;
            }
        }



        public async Task AssignTokenAsync(HttpClient client) {

            var baseAddress = client.BaseAddress.ToString();

            // update cached token if key (base address) doesn't exist or token is expired or almost expired
            if (!_tokenCache.TryGetValue(baseAddress, out CachedToken cachedToken)
                    || DateTime.Now.AddSeconds(EXPIRATION_BUFFER_IN_SECONDS) >= cachedToken.ExpiresOn) {
                var idpClient = GetIdpClient();
                var disco = await GetDiscoveryDocumentAsync(idpClient);
                await UpdateCachedTokenAsync(baseAddress, idpClient, disco);
            }

            client.SetBearerToken(_tokenCache[baseAddress].TokenResponse.AccessToken);
        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string token) {
            await Task.Run(() => { });
            var tokenHandler = new JwtSecurityTokenHandler();
            try {
                var parameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidIssuer = Options.Authority,
                    ValidateAudience = false, //false when using ApiScopes in IdentityServer
                    //ValidAudience = _options.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _jwk
                };
                var cp = tokenHandler.ValidateToken(token, parameters, out SecurityToken _);

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
            var disco = await idpClient.GetDiscoveryDocumentAsync(Options.Authority);
            if (disco.IsError) {
                _logger.LogError(disco.Error);
                return null;
            }
            return disco;
        }

        private async Task UpdateCachedTokenAsync(string baseAddress, HttpClient idpClient, DiscoveryDocumentResponse disco) {

            // get token
            var tokenResponse = await idpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                Address = disco.TokenEndpoint,

                ClientId = Options.ClientId,
                ClientSecret = Options.ClientSecret,
                Scope = string.Join(" ", Options.Scopes)
            });

            if (tokenResponse.IsError) {
                _logger.LogError(disco.Error);
                return;
            }

            var cachedToken = new CachedToken {
                TokenResponse = tokenResponse,
                ExpiresOn = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
            };

            _tokenCache.AddOrUpdate(baseAddress, cachedToken, (k, v) => _tokenCache[k] = v);


        }
    }


}
