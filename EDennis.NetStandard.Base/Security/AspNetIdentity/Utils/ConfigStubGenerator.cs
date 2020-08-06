using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace EDennis.NetStandard.Base.Security.AspNetIdentity.Utils {

    public enum IdpConfigType {
        ClientCredentials,
        AuthorizationCode
    }

    public static class ConfigStubGenerator {

        public const string OUTPUT_DIR = "IdpConfig";
        public const string DEFAULT_SECRET = "secret";
        public const string DEFAULT_CLIENT_CLAIMS_PREFIX = "";

        public readonly static List<TestUser> DEFAULT_USERS 
            = new List<TestUser> {
                new TestUser {
                    Email = "admin1@.test",
                    OrganizationName = "a.test",
                    Claims = new Dictionary<string, List<string>> {
                        { "name", new List<string>() { "Maria" } },
                        { "phone", new List<string>() { "999.555.1212" } }
                    },
                    Roles = new List<string>() { "Admin" }
                },
                new TestUser {
                    Email = "darius@b.test",
                    OrganizationName = "b.test",
                    Claims = new Dictionary<string, List<string>> {
                        { "name", new List<string>() { "Darius" } },
                        { "phone", new List<string>() { "888.555.1212" } }
                    },
                    Roles = new List<string>() { "User" }
                },
                new TestUser {
                    Email = "pat@c.test",
                    OrganizationName = "c.test",
                    Claims = new Dictionary<string, List<string>> {
                        { "name", new List<string>() { "Pat" } },
                        { "phone", new List<string>() { "777.555.1212" } }
                    },
                    Roles = new List<string>() { "Readonly" }
                },
            };

        public static void GenerateIdpConfigStub<TStartup>(
            IdpConfigType idpConfigType = IdpConfigType.ClientCredentials,
            string idpPortOrUrl = "5001",
            string apiPortOrUrl = "6001",
            IEnumerable<TestUser> testUsers = null) {


            if (!Directory.Exists(OUTPUT_DIR))
                Directory.CreateDirectory(OUTPUT_DIR);

            var assembly = typeof(TStartup).Assembly;
            var scopes = GenerateScopes(assembly, out string project);

            var users = testUsers ?? DEFAULT_USERS;
            var userClaims = users.SelectMany(u => u.Claims).Select(u => u.Key).Distinct().ToList();

            using var fs = new FileStream($"{OUTPUT_DIR}\\{project}\\.json", FileMode.CreateNew);
            using var jw = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true });

            var idpUrl = idpPortOrUrl.StartsWith("http") ? idpPortOrUrl : $"https://localhost:{idpPortOrUrl}";
            var apiUrl = apiPortOrUrl.StartsWith("http") ? apiPortOrUrl : $"https://localhost:{apiPortOrUrl}";


            jw.WriteStartObject();
            {
                WriteApiResourceSection(jw, project, idpUrl, scopes, userClaims);

                if (idpConfigType == IdpConfigType.ClientCredentials)
                    WriteClientCredentialsSection(jw, project, idpUrl);
                else {
                    WriteAuthorizationCodeSection(jw, project, idpUrl, apiUrl);
                }

                WriteTestUsersSection(jw, testUsers);
            }
            jw.WriteEndObject();


        }

        private static void WriteTestUsersSection(Utf8JsonWriter jw, IEnumerable<TestUser> testUsers) {
            jw.WriteStartArray("TestUsers");
            {
                foreach(var user in testUsers) {
                    jw.WriteStartObject();
                    {
                        jw.WriteString("Email", user.Email);
                        jw.WriteString("OrganizationName", user.OrganizationName);
                        jw.WriteStartArray("Roles");
                        {
                            foreach(var role in user.Roles) {
                                jw.WriteStringValue(role);
                            }
                        }
                        jw.WriteEndArray();
                        jw.WriteStartArray("Claims");
                        {
                            foreach (var claimType in user.Claims.Keys) {
                                jw.WriteStartArray(claimType);
                                {
                                    foreach(var claimValue in user.Claims[claimType]) {
                                        jw.WriteStringValue(claimValue);
                                    }
                                }
                                jw.WriteEndArray();
                            }
                        }
                        jw.WriteEndArray();
                    }
                    jw.WriteEndObject();
                }
            }
            jw.WriteEndArray();
        }

            private static void WriteApiResourceSection(Utf8JsonWriter jw, string project, string idpUrl, List<string> scopes, List<string> userClaims) {
            jw.WriteStartObject("ApiResource");
            {
                jw.WriteString("Name", project);
                jw.WriteStartArray("Scopes");
                {
                    foreach (var scope in scopes)
                        jw.WriteStringValue(scope);
                }
                jw.WriteEndArray();
                jw.WriteStartArray("UserClaims");
                {
                    foreach (var claim in userClaims)
                        jw.WriteStringValue(claim);
                }
                jw.WriteEndArray();

            }
            jw.WriteEndObject();
        }


        private static void WriteClientCredentialsSection(Utf8JsonWriter jw, string project, string idpUrl) {
            jw.WriteStartObject("ClientCredentials");
            {
                jw.WriteString("Authority", idpUrl);
                jw.WriteString("ClientId", project);
                jw.WriteString("ClientSecret", DEFAULT_SECRET);
                jw.WriteString("ClientClaimsPrefix", DEFAULT_CLIENT_CLAIMS_PREFIX);
                jw.WriteStartArray("Scopes");
                {
                    jw.WriteStringValue($"{project}.*");
                }
                jw.WriteEndArray();
            }
            jw.WriteEndObject();
        }

        private static void WriteAuthorizationCodeSection(Utf8JsonWriter jw, string project, string idpUrl, string apiUrl) {
            jw.WriteStartObject("AuthorizationCode");
            {
                jw.WriteString("Authority", idpUrl);
                jw.WriteString("ClientId", project);
                jw.WriteString("ClientSecret", DEFAULT_SECRET);
                jw.WriteString("ClientClaimsPrefix", DEFAULT_CLIENT_CLAIMS_PREFIX);
                jw.WriteStartArray("Scopes");
                {
                    jw.WriteStringValue($"{project}.*");
                    jw.WriteStringValue("openid");
                    jw.WriteStringValue("profile");
                    jw.WriteStringValue("name");
                    jw.WriteStringValue("email");
                    jw.WriteStringValue("role");
                }
                jw.WriteEndArray();
                jw.WriteString("GrantType", "code");
                jw.WriteString("RedirectUri", $"http://localhost:{apiUrl}/signin-oidc");
                jw.WriteString("PostLogoutRedirectUri", $"http://localhost:{apiUrl}/signout-callback-oidc");
                jw.WriteBoolean("AllowOfflineAccess", true);
            }
            jw.WriteEndObject();
        }



        private static List<string> GenerateScopes(Assembly assembly, out string project) {

            var models = assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type)) //filter controllers
                .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
                .Where(method => method.IsPublic && !method.IsDefined(typeof(NonActionAttribute))
                    && !method.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
                .Select(x => KeyValuePair.Create(x.DeclaringType.Name, x.Name))
                .ToDictionary(x => (x.Key, x.Value));

            var scopes = new List<string>();
            project = assembly.GetName().Name;
            scopes.Add($"{project}.*");
            foreach (var controller in models.Keys) {
                scopes.Add($"{project}.{controller}.*");
                foreach (var action in models.Values) {
                    scopes.Add($"{project}.{controller}.{action}");
                }
            }

            return scopes;
        }


    }
}
