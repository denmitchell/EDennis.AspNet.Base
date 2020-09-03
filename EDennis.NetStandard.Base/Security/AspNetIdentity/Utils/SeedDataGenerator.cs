using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
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
            IdpConfigType idpConfigType = IdpConfigType.ClientCredentials,
            string appRolesCsv = null,
            string childClaimsCsv = null,
            string appName = null,
            IEnumerable<TestUser> testUsers = null) {

            testUsers ??= DEFAULT_USERS;

            //if using ChildClaimCache or AppRoleChildClaimCache,
            //make sure that each defined parent claim is held
            //by at least one test user.
            if (appRolesCsv != null || childClaimsCsv != null) {

                var parentRoleClaims = new List<Claim>();
                var parentClaims = new List<Claim>();

                var roles = testUsers.SelectMany(u => u.Roles).Distinct();
                var userIdx = 0;
                var users = testUsers.ToArray();

                if (appRolesCsv != null) {
                    GetParentRoleClaims(appRolesCsv, parentRoleClaims, appName);
                    foreach (var role in parentRoleClaims.Select(c => c.Value)) {
                        if (!roles.Contains(role)) {
                            users[userIdx].Roles.Add(role);
                            userIdx = (userIdx + 1) % users.Length;
                        }
                    }
                }

                if (childClaimsCsv != null) {
                    GetParentClaims(childClaimsCsv, parentClaims);
                    foreach (var role in parentClaims.Select(c => c.Value)) {
                        if (!roles.Contains(role)) {
                            users[userIdx].Roles.Add(role);
                            userIdx = (userIdx + 1) % users.Length;
                        }
                    }
                }

            }



            if (!Directory.Exists(OUTPUT_DIR))
                Directory.CreateDirectory(OUTPUT_DIR);

            var assembly = typeof(TStartup).Assembly;
            var scopes = GenerateScopes(assembly, out string project);

            //what user claims will be included in access tokens
            //via DomainIdentityProfileService when requested in client app
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
                WriteApiResourceSection(jw, project, scopes, apiUserClaims);

                if (idpConfigType == IdpConfigType.ClientCredentials)
                    WriteClientCredentialsSection(jw, project, idpUrl);
                else {
                    WriteAuthorizationCodeSection(jw, project, idpUrl, apiUrl);
                }

                WriteTestUsersSection(jw, testUsers);
            }
            jw.WriteEndObject();


        }

        private static void GetParentClaims(string childClaimsCsv, List<Claim> parentClaims) {
            var cache = new ChildClaimCache {
                ConfigFilePath = childClaimsCsv
            };
            parentClaims.AddRange(cache.GetChildClaims().Select(cc=>new Claim(cc.ParentType, cc.ParentValue)));
        }


        private static void GetParentRoleClaims(string appRoleClaimsCsv, List<Claim> parentRoleClaims, string appName) {
            var cache = new AppRoleChildClaimCache {
                ApplicationName = appName,
                ConfigFilePath = appRoleClaimsCsv
            };
            parentRoleClaims.AddRange(cache.GetAppRoleChildClaims()
                .Select(cc => new Claim(DomainClaimTypes.ApplicationRole(appName), cc.AppRole)));
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

        private static void WriteApiResourceSection(Utf8JsonWriter jw, string project, IEnumerable<string> scopes, IEnumerable<string> userClaims) {

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
                    if (userClaims != null && userClaims.Count() > 0) {
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
