using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace EDennis.NetStandard.Base {
    public class AppInitControllerClient<TStartup> {

        private readonly IConfiguration _config;
        private readonly OneTimeTokenService _tokenService;
        private readonly HttpClient _httpClient;

        public readonly static string[] DEFAULT_API_USER_SCOPES = new string[] { "name", "email", "phone" };

        public AppInitControllerClient(IConfiguration config, OneTimeTokenService tokenService) {
            _config = config;
            _tokenService = tokenService;
            _httpClient = new HttpClient { BaseAddress = new Uri(_tokenService.Options.Authority) };
            _tokenService.AssignTokenAsync(_httpClient).Wait();
        }

        public void LoadApi(string configKey) {

            ApiResourceOptions options = new ApiResourceOptions();
            _config.GetSection(configKey).Bind(options);

            if (options.Scopes == null || options.Scopes.Length == 0) {
                var assembly = GetAssembly();
                options.Scopes = GenerateScopes(assembly, options.Name);
                options.Name ??= assembly.GetName().Name;
                options.UserClaims ??= DEFAULT_API_USER_SCOPES;
            }

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


        private Assembly GetAssembly() {
            return Assembly.GetAssembly(typeof(TStartup));
        }

        private string[] GenerateScopes(Assembly assembly, string apiName = null) {


            var models = assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type)) //filter controllers
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute))
                    && !method.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                .Select(x => KeyValuePair.Create(x.DeclaringType.Name, x.Name))
                .ToDictionary(x=> (x.Key, x.Value));

            var scopes = new List<string>();
            var project = apiName ?? assembly.GetName().Name;
            scopes.Add($"{project}.*");
            foreach (var controller in models.Keys) {
                scopes.Add($"{project}.{controller}.*");
                foreach (var action in models.Values) {
                    scopes.Add($"{project}.{controller}.{action}");
                }
            }

            return scopes.ToArray();
        }

    }
}
