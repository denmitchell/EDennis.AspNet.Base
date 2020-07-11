using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    
    
    /// <summary>
    /// Singleton that caches roles and role claims for the current application.
    /// Note that the factory pattern should be used to build the singleton.
    /// </summary>
    public class RoleDependentClaimsCache : ConcurrentDictionary<Guid,IEnumerable<Claim>> {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RoleDependentClaimsCache> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IOptionsMonitor<RoleDependentClaimsCacheOptions> _options;

        public RoleDependentClaimsCache(IOptionsMonitor<RoleDependentClaimsCacheOptions> options,
            IHttpClientFactory httpClientFactory,
            ILogger<RoleDependentClaimsCache> logger,
            IWebHostEnvironment env) {

            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _env = env;

            _options = options;
            _options.OnChange(async (_options)=>await PopulateAsync());

            Task.Run(async() => await PopulateAsync());
        }

        public async Task PopulateAsync() {
            
            var client = _httpClientFactory.CreateClient("RoleDependentClaimsCache");

            var options = _options.CurrentValue;
            var disco = await client.GetDiscoveryDocumentAsync(options.Authority);
            if (disco.IsError) {
               _logger.LogError(disco.Error);
                return;
            }

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                Address = disco.TokenEndpoint,

                ClientId = options.ClientId,
                ClientSecret = options.ClientSecret,
                Scope = options.Scope
            });

            if (tokenResponse.IsError) {
                _logger.LogError(tokenResponse.Error);
                return;
            }

            client.SetBearerToken(tokenResponse.AccessToken);
            var url = options.Endpoint;
            if (url.EndsWith("/"))
                url = url[0..^1];
            
            var response = await client.GetAsync($"{url}/{_env.ApplicationName}");
            
            if (!response.IsSuccessStatusCode) {
                _logger.LogError($"RoleDependentClaimsCache could not retrieve role data from {url}/{_env.ApplicationName}");
            } else {
                var json = await response.Content.ReadAsStringAsync();
                var drc = JsonSerializer.Deserialize<DomainRolesAndClaims>(json);

                Clear();

                foreach(var role in drc.DomainRoles) {
                    var claims = new List<Claim> {
                        new Claim("role", role.Name)
                    };
                    foreach (var roleClaim in drc.AspNetRoleClaims.Where(rc => rc.RoleId == role.Id))
                        claims.Add(new Claim(roleClaim.ClaimType, roleClaim.ClaimValue));
                    if (!TryAdd(role.Id, claims))
                        _logger.LogError($"Could not populate RoleDependentClaimsCache");
                }

            }



        }

    }
}
