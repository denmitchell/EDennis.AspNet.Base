using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace EDennis.NetStandard.Base {

    public enum IdpConfigType {
        ClientCredentials,
        AuthorizationCode
    }

    public static class SeedDataGenerator {

        public const string OUTPUT_DIR = "IdpConfig";
        public const string DEFAULT_SECRET = "secret";
        public const string DEFAULT_CLIENT_CLAIMS_PREFIX = "";

        public readonly static List<TestUser> DEFAULT_USERS
            = new List<TestUser> {
                new TestUser {
                    Email = "maria@a.test",
                    PhoneNumber = "999.555.1212",
                    Roles = new List<string>() { "Admin" }
                },
                new TestUser {
                    Email = "john@a.test",
                    OrganizationAdmin = true,
                    PhoneNumber = "999.555.1313",
                    Roles = new List<string>() { "User" }
                },
                new TestUser {
                    Email = "darius@b.test",
                    OrganizationAdmin = true,
                    PhoneNumber = "888.555.1212",
                    Roles = new List<string>() { "User" }
                },
                new TestUser {
                    Email = "linda@b.test",
                    PhoneNumber = "888.555.1313",
                    Roles = new List<string>() { "Readonly" }
                },
                new TestUser {
                    Email = "pat@c.test",
                    OrganizationAdmin = true,
                    PhoneNumber = "777.555.1212",
                    Roles = new List<string>() { "User" }
                },
                new TestUser {
                    Email = "ebony@c.test",
                    PhoneNumber = "777.555.1313",
                    Roles = new List<string>() { "Readonly" }
                },
                new TestUser {
                    Email="juan@a.test",
                    PhoneNumber = "999.555.1414",
                    Claims = new Dictionary<string,List<string>>() {
                        { "*:role", new List<string>{ "*.admin" } }
                    }
                },
                new TestUser {
                    Email="james@b.test",
                    PhoneNumber = "888.555.1414",
                    Roles = new List<string>() { "Readonly" },
                    LockedOut = true
                }
            };


        public static void GenerateIdpConfigStub<TStartup>(
            dynamic idpPortOrUrl,
            dynamic apiPortOrUrl,
            IdpConfigType idpConfigType = IdpConfigType.ClientCredentials,
            IEnumerable<TestUser> testUsers = null) {


            testUsers ??= DEFAULT_USERS;

            if (!Directory.Exists(OUTPUT_DIR))
                Directory.CreateDirectory(OUTPUT_DIR);

            var assembly = typeof(TStartup).Assembly;
            var scopes = GenerateScopes(assembly, out string project);

            var users = testUsers ?? DEFAULT_USERS;
            var userClaims = users.SelectMany(u => u.Claims)
                .Select(u => u.Key)
                .Distinct()
                .Union(new string[] { "role" })
                .ToList();

            var path = $"{OUTPUT_DIR}\\{project}.json";
            if (File.Exists(path))
                File.Delete(path);

            using var fs = new FileStream(path, FileMode.CreateNew);
            using var jw = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true });

            var idpUrl = typeof(int) == idpPortOrUrl.GetType() ? $"https://localhost:{idpPortOrUrl}" : $"{idpPortOrUrl}";
            var apiUrl = typeof(int) == apiPortOrUrl.GetType() ? $"https://localhost:{apiPortOrUrl}" : $"{apiPortOrUrl}";


            jw.WriteStartObject();
            {
                WriteApiResourceSection(jw, project, scopes, userClaims);

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
                foreach (var user in testUsers) {
                    jw.WriteStartObject();
                    {
                        jw.WriteString("Email", user.Email);
                        if(user.EmailConfirmed != TestUser.EMAIL_CONFIRMED_DEFAULT)
                            jw.WriteBoolean("EmailConfirmed", user.EmailConfirmed);
                        if (user.PlainTextPassword != TestUser.PASSWORD_DEFAULT)
                            jw.WriteString("PlainTextPassword", user.PlainTextPassword);
                        if (user.PhoneNumber != TestUser.PHONE_NUMBER_DEFAULT)
                            jw.WriteString("PhoneNumber", user.PhoneNumber);
                        if(user.PhoneNumberConfirmed != TestUser.PHONE_CONFIRMED_DEFAULT)
                            jw.WriteBoolean("PhoneNumberConfirmed", user.PhoneNumberConfirmed);
                        if (user.Organization != default)
                            jw.WriteString("Organization", user.Organization);
                        if (user.OrganizationConfirmed != TestUser.ORGANIZATION_CONFIRMED_DEFAULT)
                            jw.WriteBoolean("OrganizationConfirmed", user.OrganizationConfirmed);
                        if (user.OrganizationAdmin != TestUser.ORGANIZATION_ADMIN_DEFAULT)
                            jw.WriteBoolean("OrganizationAdmin", user.OrganizationAdmin);
                        if (user.LockedOut != TestUser.LOCKED_OUT_DEFAULT)
                            jw.WriteBoolean("LockedOut", user.LockedOut);

                        if (user.Roles != null && user.Roles.Count > 0) {
                            jw.WriteStartArray("Roles");
                            {
                                foreach (var role in user.Roles) {
                                    jw.WriteStringValue(role);
                                }
                            }
                            jw.WriteEndArray();
                        }

                        if (user.Claims != null && user.Claims.Count > 0) {
                            jw.WriteStartObject("Claims");
                            {
                                foreach (var claimType in user.Claims.Keys) {
                                    jw.WriteStartArray(claimType);
                                    {
                                        foreach (var claimValue in user.Claims[claimType]) {
                                            jw.WriteStringValue(claimValue);
                                        }
                                    }
                                    jw.WriteEndArray();
                                }
                            }
                            jw.WriteEndObject();
                        }

                    }
                    jw.WriteEndObject();
                }
            }
            jw.WriteEndArray();
        }

        private static void WriteApiResourceSection(Utf8JsonWriter jw, string project, List<string> scopes, List<string> userClaims) {

            jw.WriteStartArray("ApiResources");
            {
                jw.WriteStartObject();
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
            jw.WriteEndArray();
        }


        private static void WriteClientCredentialsSection(Utf8JsonWriter jw, string project, string idpUrl) {
            jw.WriteStartArray("Clients");
            {
                jw.WriteStartObject();
                {
                    jw.WriteString("Authority", idpUrl);
                    jw.WriteString("ClientId", project);
                    jw.WriteString("PlainTextSecret", DEFAULT_SECRET);
                    jw.WriteStartArray("AllowedGrantTypes");
                    {
                        jw.WriteStringValue("client_credentials");
                    }
                    jw.WriteEndArray();
                    jw.WriteString("ClientClaimsPrefix", DEFAULT_CLIENT_CLAIMS_PREFIX);
                    jw.WriteStartArray("AllowedScopes");
                    {
                        jw.WriteStringValue($"{project}.*");
                    }
                    jw.WriteEndArray();
                    jw.WriteStartArray("Applications");
                    {
                        jw.WriteStringValue(project);
                    }
                    jw.WriteEndArray();
                }
                jw.WriteEndObject();
            }
            jw.WriteEndArray();
        }

        private static void WriteAuthorizationCodeSection(Utf8JsonWriter jw, string project, string idpUrl, string apiUrl) {
            jw.WriteStartArray("Clients");
            {
                jw.WriteStartObject();
                {
                    jw.WriteString("Authority", idpUrl);
                    jw.WriteString("ClientId", project);
                    jw.WriteString("PlainTextSecret", DEFAULT_SECRET);
                    jw.WriteStartArray("AllowedGrantTypes");
                    {
                        jw.WriteStringValue("code");
                    }
                    jw.WriteEndArray();
                    jw.WriteBoolean("RequireConsent", false);
                    jw.WriteBoolean("RequirePkce", true);
                    jw.WriteBoolean("AllowOfflineAccess", true);
                    jw.WriteString("ClientClaimsPrefix", DEFAULT_CLIENT_CLAIMS_PREFIX);
                    jw.WriteStartArray("AllowedScopes");
                    {
                        jw.WriteStringValue($"{project}.*");
                        jw.WriteStringValue("openid");
                        jw.WriteStringValue("profile");
                        jw.WriteStringValue("name");
                        jw.WriteStringValue("email");
                        jw.WriteStringValue("role");
                    }
                    jw.WriteEndArray();
                    jw.WriteStartArray("RedirectUris");
                    {
                        jw.WriteStringValue($"{apiUrl}/signin-oidc");
                    }
                    jw.WriteEndArray();
                    jw.WriteStartArray("PostLogoutRedirectUris");
                    {
                        jw.WriteStringValue($"{apiUrl}/signout-callback-oidc");
                    }
                    jw.WriteEndArray();
                    jw.WriteStartArray("Applications");
                    {
                        jw.WriteStringValue(project);
                    }
                    jw.WriteEndArray();
                }
                jw.WriteEndObject();
            }
            jw.WriteEndArray();
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

            scopes.Add($"{project}.*.Get*");
            scopes.Add($"{project}.*.Post*");
            scopes.Add($"{project}.*.Put*");
            scopes.Add($"{project}.*.Patch*");
            scopes.Add($"{project}.*.Delete*");

            foreach (var controller in models.Keys) {
                scopes.Add($"{project}.{controller}.*");
                scopes.Add($"{project}.{controller}.Get*");
                scopes.Add($"{project}.{controller}.Post*");
                scopes.Add($"{project}.{controller}.Put*");
                scopes.Add($"{project}.{controller}.Patch*");
                scopes.Add($"{project}.{controller}.Delete*");
                var actions = models[controller].Where(a =>
                    !a.StartsWith("Get") &&
                    !a.StartsWith("Post") &&
                    !a.StartsWith("Put") &&
                    !a.StartsWith("Patch") &&
                    !a.StartsWith("Delete"))
                    ;
                foreach (var action in actions) {
                    scopes.Add($"{project}.{controller}.{action}");
                }
            }

            return scopes;
        }


    }
}
