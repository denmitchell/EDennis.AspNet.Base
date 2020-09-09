using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
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
                    OrganizationAdmin = true,
                    Roles = new List<string>() { "admin" }
                },
                new TestUser {
                    Email = "john@a.test",
                    PhoneNumber = "999.555.1313",
                    Roles = new List<string>() { "user" }
                },
                new TestUser {
                    Email = "darius@b.test",
                    PhoneNumber = "888.555.1212",
                    OrganizationAdmin = true,
                    Roles = new List<string>() { "user" }
                },
                new TestUser {
                    Email = "linda@b.test",
                    PhoneNumber = "888.555.1313",
                    Roles = new List<string>() { "readonly" }
                },
                new TestUser {
                    Email = "pat@c.test",
                    PhoneNumber = "777.555.1212",
                    OrganizationAdmin = true,
                    Roles = new List<string>() { "user" }
                },
                new TestUser {
                    Email = "ebony@c.test",
                    PhoneNumber = "777.555.1313",
                    Roles = new List<string>() { "readonly" }
                },
                new TestUser {
                    Email="juan@a.test",
                    PhoneNumber = "999.555.1414",
                    SuperAdmin = true,
                    Claims = new Dictionary<string,List<string>>() {
                        { "someClaimType", new List<string>{ "someClaimValue" } }
                    }
                },
                new TestUser {
                    Email="james@b.test",
                    PhoneNumber = "888.555.1414",
                    Roles = new List<string>() { "readonly" },
                    LockedOut = true
                }
            };


        public static void GenerateIdpConfigStub<TStartup>(
            dynamic idpPortOrUrl,
            dynamic apiPortOrUrl,
            bool isRazorUserApp,
            string[] childApiScopes = null,
            string childClaimsConfigKey = null,
            IEnumerable<TestUser> testUsers = null) {

            childApiScopes ??= new string[] { };
            testUsers ??= DEFAULT_USERS;

            //if using ChildClaimCache, make sure that each defined 
            //parent claim is held by at least one test user.
            if (childClaimsConfigKey != null) {

                var parentClaims = new List<Claim>();

                var roles = testUsers.SelectMany(u => u.Roles).Distinct();
                var userIdx = 0;
                var users = testUsers.ToArray();

                GetParentClaims(childClaimsConfigKey, parentClaims);
                foreach (var role in parentClaims.Select(c => c.Value)) {
                    if (!roles.Contains(role)) {
                        users[userIdx].Roles.Add(role);
                        userIdx = (userIdx + 1) % users.Length;
                    }
                }

            }



            if (!Directory.Exists(OUTPUT_DIR))
                Directory.CreateDirectory(OUTPUT_DIR);

            var assembly = typeof(TStartup).Assembly;
            var scopes = GenerateScopes(assembly, out string project);

            //what user claims will be included in access tokens
            var apiUserClaims =
                new string[] {
                    JwtClaimTypes.Name,
                    ClaimTypes.Name,
                    DomainClaimTypes.Organization,
                    DomainClaimTypes.ApplicationRole(project),
                    DomainClaimTypes.OrganizationAdminFor,
                    DomainClaimTypes.SuperAdmin
                };

            var path = $"{OUTPUT_DIR}\\{project}.json";
            if (File.Exists(path))
                File.Delete(path);

            using var fs = new FileStream(path, FileMode.CreateNew);
            using var jw = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true });

            var idpUrl = typeof(int) == idpPortOrUrl.GetType() ? $"https://localhost:{idpPortOrUrl}" : $"{idpPortOrUrl}";
            var apiUrl = typeof(int) == apiPortOrUrl.GetType() ? $"https://localhost:{apiPortOrUrl}" : $"{apiPortOrUrl}";


            jw.WriteStartObject();
            {
                WriteApiResourcesSection(jw, project, scopes, apiUserClaims, isRazorUserApp);

                if (childApiScopes.Any() || isRazorUserApp) {
                    jw.WriteStartArray("Clients");
                    {
                        if (childApiScopes.Any())
                            WriteClientsSectionForClientCredentials(jw, project, idpUrl, childApiScopes);
                        if (isRazorUserApp)
                            WriteClientsSectionForAuthorizationCode(jw, project, idpUrl, apiUrl);
                    }
                    jw.WriteEndArray();
                }

                WriteTestUsersSection(jw, testUsers);
            }
            jw.WriteEndObject();

        }


        /// <summary>
        /// Adds parent claims from child claims defined in configuration
        /// </summary>
        /// <param name="configKey"></param>
        /// <param name="parentClaims"></param>
        /// <param name="appName"></param>
        private static void GetParentClaims(string configKey, List<Claim> parentClaims) {
            var settings = new ChildClaimSettings();
            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
                .AddJsonFile("appsettings.json", true)
                .Build();
            config.BindSectionOrThrow(configKey, settings);

            var options = IOptionsMonitorFactory.Create(settings);
            var cache = new ChildClaimCache(options);

            parentClaims.AddRange(cache.ChildClaims
                .Select(cc => new Claim(cc.ParentType, cc.ParentValue)));
        }


        private static void WriteTestUsersSection(Utf8JsonWriter jw, IEnumerable<TestUser> testUsers) {
            jw.WriteStartArray("TestUsers");
            {
                foreach (var user in testUsers) {
                    jw.WriteStartObject();
                    {
                        jw.WriteString("Email", user.Email);
                        if (user.EmailConfirmed != TestUser.EMAIL_CONFIRMED_DEFAULT)
                            jw.WriteBoolean("EmailConfirmed", user.EmailConfirmed);
                        if (user.PlainTextPassword != TestUser.PASSWORD_DEFAULT)
                            jw.WriteString("PlainTextPassword", user.PlainTextPassword);
                        if (user.PhoneNumber != TestUser.PHONE_NUMBER_DEFAULT)
                            jw.WriteString("PhoneNumber", user.PhoneNumber);
                        if (user.PhoneNumberConfirmed != TestUser.PHONE_CONFIRMED_DEFAULT)
                            jw.WriteBoolean("PhoneNumberConfirmed", user.PhoneNumberConfirmed);
                        if (user.Organization != default)
                            jw.WriteString("Organization", user.Organization);
                        if (user.OrganizationConfirmed != TestUser.ORGANIZATION_CONFIRMED_DEFAULT)
                            jw.WriteBoolean("OrganizationConfirmed", user.OrganizationConfirmed);
                        if (user.OrganizationAdmin != TestUser.ORGANIZATION_ADMIN_DEFAULT)
                            jw.WriteBoolean("OrganizationAdmin", user.OrganizationAdmin);
                        if (user.SuperAdmin != TestUser.SUPER_ADMIN_DEFAULT)
                            jw.WriteBoolean("SuperAdmin", user.SuperAdmin);
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
                            if (user.Claims.Count == 1 && user.Claims.Keys.First() == "someClaimType")
                                jw.WriteCommentValue("Sample Custom Claims");

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

        private static void WriteApiResourcesSection(Utf8JsonWriter jw, string project, IEnumerable<string> scopes, IEnumerable<string> userClaims,
                bool hasAuthorizationCodeClient) {

            jw.WriteStartArray("ApiResources");
            {
                jw.WriteStartObject();
                {
                    jw.WriteString("Name", project);
                    if (scopes != null && scopes.Count() > 0) {
                        jw.WriteStartArray("Scopes");
                        {
                            foreach (var scope in scopes)
                                jw.WriteStringValue(scope);
                        }
                        jw.WriteEndArray();
                    }
                    if (userClaims != null && userClaims.Count() > 0 && hasAuthorizationCodeClient) {
                        jw.WriteStartArray("UserClaims");
                        {
                            foreach (var claim in userClaims)
                                jw.WriteStringValue(claim);
                        }
                        jw.WriteEndArray();
                    }

                }
                jw.WriteEndObject();
            }
            jw.WriteEndArray();
        }


        private static void WriteClientsSectionForClientCredentials(Utf8JsonWriter jw, string project, string idpUrl, string[] childApiScopes) {
            jw.WriteStartObject();
            {
                jw.WriteString("Authority", idpUrl);
                jw.WriteString("ClientId", $"{project}:API");
                jw.WriteString("PlainTextSecret", DEFAULT_SECRET);
                jw.WriteStartArray("AllowedGrantTypes");
                {
                    jw.WriteStringValue("client_credentials");
                }
                jw.WriteEndArray();
                jw.WriteString("ClientClaimsPrefix", DEFAULT_CLIENT_CLAIMS_PREFIX);
                jw.WriteStartArray("AllowedScopes");
                {
                    foreach (var scope in childApiScopes) {
                        jw.WriteStringValue(scope);
                    }
                }
                jw.WriteEndArray();
            }
            jw.WriteEndObject();
        }

        private static void WriteClientsSectionForAuthorizationCode(Utf8JsonWriter jw, string project, string idpUrl, string apiUrl) {
            jw.WriteStartObject();
            {
                jw.WriteString("Authority", idpUrl);
                jw.WriteString("ClientId", $"{project}:ID");
                jw.WriteString("PlainTextSecret", DEFAULT_SECRET);
                jw.WriteStartArray("AllowedGrantTypes");
                {
                    jw.WriteStringValue("code");
                    //jw.WriteStringValue("client_credentials");
                }
                jw.WriteEndArray();
                jw.WriteBoolean("RequireConsent", false);
                jw.WriteBoolean("RequirePkce", true);
                jw.WriteBoolean("AllowOfflineAccess", true);
                jw.WriteString("ClientClaimsPrefix", DEFAULT_CLIENT_CLAIMS_PREFIX);
                jw.WriteStartArray("AllowedScopes");
                {
                    jw.WriteStringValue("openid");
                    jw.WriteStringValue("profile");
                    jw.WriteStringValue("name");
                    jw.WriteStringValue("email");
                    jw.WriteStringValue("email_confirmed");
                    jw.WriteStringValue("offline_access");
                    jw.WriteStringValue("phone_number");
                    jw.WriteStringValue("organization");
                    jw.WriteStringValue("organization_confirmed");
                    jw.WriteStringValue("organization_admin_for");
                    jw.WriteStringValue("super_admin");
                    jw.WriteStringValue($"role:{project}");
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
