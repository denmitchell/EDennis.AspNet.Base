using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace EDennis.NetStandard.Base.Security.AspNetIdentity.ControllerClients {
    public class AppInitControllerClient {

        private readonly IConfiguration _config;
        private readonly ClientCredentialsTokenService _tokenService;
        private readonly HttpClient _httpClient;
        public AppInitControllerClient(IConfiguration config, ClientCredentialsTokenService tokenService) {
            _config = config;
            _tokenService = tokenService;
            _httpClient = new HttpClient { BaseAddress = new Uri(_tokenService.Options.Authority) };
            _tokenService.AssignTokenAsync(_httpClient).Wait();
        }

        public void LoadApi(string configKey) {
            ApiResourceOptions options = new ApiResourceOptions();
            _config.GetSection(configKey).Bind(options);
            if (options.Name == default)
                throw new Exception($"Could not bind {configKey} in Configuration to ApiResourceOptions.");
            _httpClient.PostAsync("/api", options).Wait();
        }


        public void LoadClientFromClientCredentialsOptions(string configKey) {
            ClientCredentialsOptions options = new ClientCredentialsOptions();
            _config.GetSection(configKey).Bind(options);
            if (options.ClientId == default)
                throw new Exception($"Could not bind {configKey} in Configuration to ClientCredentialsOptions.");
            _httpClient.PostAsync("/client/cc", options).Wait();
        }

        public void LoadClientFromOidcOptions(string configKey) {
            OidcOptions options = new OidcOptions();
            _config.GetSection(configKey).Bind(options);
            if (options.ClientId == default)
                throw new Exception($"Could not bind {configKey} in Configuration to OidcOptions.");
            _httpClient.PostAsync("/client/oidc", options).Wait();
        }

        public void LoadTestUsers(string configKey) {
            List<TestUser> users = new List<TestUser>();
            _config.GetSection(configKey).Bind(users);
            if (users.Count == 0)
                throw new Exception($"Could not bind {configKey} in Configuration to List<TestUser>.");
            _httpClient.PostAsync("/users", users).Wait();
        }

    }
}
