using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using E = IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EDennis.AspNetIdentityServer {

    /// <summary>
    /// Seeds IdentityServer and ASP.NET Identity databases 
    /// from JSON configuration files
    /// </summary>
    public class SeedDataLoader {

        public const string DEFAULT_SEED_DATA_DIR = "SeedData";
        public const string DEFAULT_DBVIEW_DIR = "Logs";
        public const string DEFAULT_DBVIEW_FILE = "DbView.json";
        public const string DEFAULT_IDENTITY_RESOURCES_FILE = "IdentityResources.json";


        public string SeedDataDir { get; set; } = DEFAULT_SEED_DATA_DIR;
        public string DbViewDir { get; set; } = DEFAULT_DBVIEW_DIR;
        public string DbViewFile { get; set; } = DEFAULT_DBVIEW_FILE;
        public string IdentityResourcesFile { get; set; } = DEFAULT_IDENTITY_RESOURCES_FILE;

        private readonly IHost _host;
        private readonly string[] _args;
        private readonly ILogger _logger;


        public Regex FileProjectExtractor { get; set; }
                = new Regex($@"(?<={DEFAULT_SEED_DATA_DIR}\\)([A-Za-z0-9_.]+)(?=\.json)");


        public SeedDataLoader(IHost host, string[] args, ILogger logger) {
            _host = host;
            _args = args;
            _logger = logger;
            Initialize();
        }

        public SeedDataLoader(IHost host, string[] args, Serilog.ILogger logger) {
            _host = host;
            _args = args;
            _logger = new SerilogLoggerProvider(logger).CreateLogger(nameof(SeedDataLoader));
            Initialize();
        }

        public bool HasProjects { get; set; }

        private IEnumerable<string> _projects { get; set; }
        private bool _print;
        private bool _commit;

        private void Initialize() {

            if (_args.Contains("/migrate"))
                MigrateDb(_host, _args);

            var configs = Directory.EnumerateFiles(SeedDataDir);

            if (_args.Contains("/all"))
                _projects = configs.Where(c => c != $"{SeedDataDir}\\{IdentityResourcesFile}")
                    .Select(c => FileProjectExtractor.Match(c).Value);
            else
                _projects = _args.Select(a => a[1..]).Where(a => configs.Contains($"{SeedDataDir}\\{a}.json"));

            _print = _args.Contains("/print");
            _commit = _args.Contains("/commit");

            HasProjects = _projects.Count() > 0;
        }


        public void Load() {

            if (!HasProjects)
                return;

            using var scope = _host.Services.CreateScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var cxnString = config["ConnectionStrings:DomainIdentityDbContext"];

            using var cxn = new SqlConnection(cxnString);
            cxn.Open();
            using var trans = cxn.BeginTransaction();

            var diContext = GetContext<DomainIdentityDbContext>(cxn, trans);
            var cContext = GetContext<ConfigurationDbContext>(cxn, trans);
            var pContext = GetContext<PersistedGrantDbContext>(cxn, trans);

            LoadConfigs(_projects, cContext, diContext);

            _logger.LogInformation("**Finished Loading Configurations.");

            if (_commit)
                trans.Commit();

            if (_print)
                PrintDb(_host, diContext, trans);

        }



        private void LoadConfigs(IEnumerable<string> projects,
            ConfigurationDbContext cContext, DomainIdentityDbContext diContext) {

            _logger.LogInformation($"Loading {SeedDataDir}\\{IdentityResourcesFile} ...");

            var irConfig = new ConfigurationBuilder()
                    .AddJsonFile($"{SeedDataDir}\\{IdentityResourcesFile}")
                    .Build();
            var ids = new List<IdentityResource>();
            irConfig.GetSection("IdentityResources").Bind(ids);

            if (ids.Count == 0)
                throw new Exception($"Could not find/bind {SeedDataDir}\\{IdentityResourcesFile}");


            foreach (var resource in ids) {
                var existing = cContext.IdentityResources.FirstOrDefault(x => x.Name == resource.Name);
                if (existing == null) {
                    _logger.LogInformation($"\tAdding IdentityResource {resource.Name}");
                    var entity = resource.ToEntity();
                    cContext.IdentityResources.Add(entity);
                }
            }
            cContext.SaveChanges();


            _logger.LogInformation($"Loading project configs from {SeedDataDir} ...");

            foreach (var project in projects) {

                var config = new ConfigurationBuilder()
                    .AddJsonFile($"{SeedDataDir}\\{project}.json")
                    .Build();


                _logger.LogInformation($"\tEnsuring Identity Resource for role:{project} claim ...");

                //ensure that the project's role claim is represented as a standalone identity resource and user claim
                var idUserClaims = cContext.IdentityResources.SelectMany(a => a.UserClaims).Select(c => c.Type).Distinct();
                if (!idUserClaims.Any(a => a == $"role:{project}")) {
                    cContext.IdentityResources.Add(
                        new IdentityResource {
                            Name = $"role:{project}",
                            UserClaims = new string[] { $"role:{project}" }
                        }.ToEntity());
                }



                _logger.LogInformation($"\tLoading Apis for {project} ...");
                var apis = new List<ApiResource>();
                config.GetSection("ApiResources").Bind(apis);

                if (apis.Count > 0) {

                    foreach (var api in apis) {
                        _logger.LogInformation($"\t\tLoading {api.Name} ...");
                        //add claims (mainly role claims) to existing ApiResources
                        var existing = cContext.ApiResources.FirstOrDefault(x => x.Name == api.Name);
                        if (existing != null) {
                            foreach (var claim in api.UserClaims) {
                                if (!existing.UserClaims.Any(u => u.Type == claim))
                                    existing.UserClaims.Add(new E.ApiResourceClaim {
                                        ApiResourceId = existing.Id,
                                        Type = claim
                                    });
                                cContext.SaveChanges();
                            }
                            //cContext.ApiResources.Remove(existing);
                            //cContext.SaveChanges();
                        }
                        foreach (var scope in api.Scopes)
                            if (!cContext.ApiScopes.Any(x => x.Name == scope))
                                cContext.ApiScopes.Add(new ApiScope { Name = scope, Description = scope }.ToEntity());

                        
                        cContext.ApiResources.Add(api.ToEntity());

                    }
                    cContext.SaveChanges();
                }

                _logger.LogInformation($"\tLoading Clients for {project} ...");
                var clients = new List<ClientConfig>();
                config.GetSection("Clients").Bind(clients);
                //if (clients.Count == 0)
                //    throw new Exception($"Could not find/bind Clients section in {SeedDataDir}\\{project}");

                foreach (var client in clients) {
                    _logger.LogInformation($"\t\tLoading {client.ClientId} ...");
                    var existing = cContext.Clients.FirstOrDefault(x => x.ClientId == client.ClientId);
                    if (existing != null) {
                        cContext.Clients.Remove(existing);
                        cContext.SaveChanges();
                    }
                    cContext.Clients.Add(client.ToEntity());
                }
                cContext.SaveChanges();

                var users = new List<TestUser>();
                config.GetSection("TestUsers").Bind(users);
                if (users.Count == 0) {
                    _logger.LogInformation($"\tNo TestUsers found for {project} ...");
                } else {
                    _logger.LogInformation($"\tLoading TestUsers for {project} ...");
                    LoadTestUsers(diContext, project, users);
                }
            }

        }



        private void LoadTestUsers(DomainIdentityDbContext context, string project, List<TestUser> users) {
            _logger.LogInformation($"\t\tChecking application record for {project} ...");
            var app = context.Set<DomainApplication>().FirstOrDefault(a => a.Name == project);
            if (app != null) {
                _logger.LogInformation($"\t\tApplication record found for {project} ...");
            } else {
                _logger.LogInformation($"\t\tAdding application record for {project} ...");
                app = new DomainApplication {
                    Name = project
                };
                context.Set<DomainApplication>().Add(app);
                context.SaveChanges();
            }



            _logger.LogInformation($"\t\tAdding any missing application claim records ...");

            var roles = users.SelectMany(u => u.Roles)
                .Distinct()
                .ToDictionary(r => r, r => default(int));

            var applicationClaimRecords = context.ApplicationClaims
                .Where(ac => ac.Application == project
                    && ac.ClaimTypePrefix == "role:")
                .ToList();

            var missingApplicationClaims = roles.Keys
                .Where(r => !applicationClaimRecords
                .Any(ac => ac.ClaimValue == r))
                .Select(r => new DomainApplicationClaim {
                    Application = project,
                    ClaimTypePrefix = "role:",
                    ClaimValue = r,
                    OrgAdminable = true
                });

            context.ApplicationClaims.AddRange(missingApplicationClaims);
            context.SaveChanges();



            _logger.LogInformation($"\t\t\tAdding any missing organization records ...");

            for (int i = 0; i < users.Count(); i++)
                users[i].Organization = users[i].Organization ?? users[i].Email.Substring(users[i].Email.IndexOf('@') + 1);

            var orgs = users.Select(u => u.Organization).Distinct();

            var existingOrgs = context.Set<DomainOrganization>()
                .Select(o => o.Name).ToList();

            var missingOrgs = orgs.Except(existingOrgs)
                .Select(o => new DomainOrganization { Name = o });

            context.Organizations.AddRange(missingOrgs);
            context.SaveChanges();



            _logger.LogInformation($"\t\t\tAdding any missing organization application records ...");

            var orgApps = orgs.Select(o => new DomainOrganizationApplication { Organization = o, Application = project });

            var existingOrgApps = context.OrganizationApplications
                .Where(oa => oa.Application == project);

            var missingOrgApps = orgApps.Except(existingOrgApps);

            context.OrganizationApplications.AddRange(missingOrgApps);
            context.SaveChanges();



            _logger.LogInformation($"\t\tChecking test user records for {project} ...");

            foreach (var entry in users) {


                _logger.LogInformation($"\t\t\tChecking user record for {entry.Email} ...");

                int userId = default;
                var user = context.Users.FirstOrDefault(a => a.Email == entry.Email);
                if (user != null) {
                    userId = user.Id;
                    _logger.LogInformation($"\t\t\tUser record found for {entry.Email} ...");
                } else {
                    _logger.LogInformation($"\t\t\tAdding user record for {entry.Email} ...");
                    user = new DomainUser {
                        Email = entry.Email,
                        NormalizedEmail = entry.Email,
                        UserName = entry.Email,
                        NormalizedUserName = entry.Email,
                        EmailConfirmed = entry.EmailConfirmed,
                        PhoneNumber = entry.PhoneNumber,
                        PhoneNumberConfirmed = entry.PhoneNumberConfirmed,
                        Organization = entry.Organization ?? entry.Email.Substring(entry.Email.IndexOf('@') + 1),
                        OrganizationConfirmed = entry.OrganizationConfirmed,
                        OrganizationAdmin = entry.OrganizationAdmin,
                        SuperAdmin = entry.SuperAdmin,
                        LockoutBegin = entry.LockedOut ? (DateTimeOffset?)DateTime.MinValue.ToUniversalTime() : null,
                        LockoutEnd = entry.LockedOut ? (DateTimeOffset?)DateTime.MaxValue.ToUniversalTime() : null,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };
                    user.PasswordHash = HashPassword(entry.PlainTextPassword);
                    context.Users.Add(user);
                    context.SaveChanges();
                    userId = user.Id;
                }


                _logger.LogInformation($"\t\t\tChecking claim records for {entry.Email} ...");

                foreach (var claim in entry.Claims)
                    foreach (var value in claim.Value) {
                        if (!context.UserClaims.Any(ur => ur.UserId == userId && ur.ClaimType == claim.Key && ur.ClaimValue == value)) {
                            context.UserClaims.Add(new IdentityUserClaim<int> {
                                UserId = userId,
                                ClaimType = claim.Key,
                                ClaimValue = value
                            });
                            context.SaveChanges();
                        }
                    }

                _logger.LogInformation($"\t\t\tChecking application role records for {entry.Email} ...");

                foreach (var role in entry.Roles)
                    if (!context.UserClaims.Any(ur => ur.UserId == userId && ur.ClaimType == $"role:{project}" && ur.ClaimValue == role)) {
                        context.UserClaims.Add(new IdentityUserClaim<int> {
                            UserId = userId,
                            ClaimType = $"role:{project}",
                            ClaimValue = role
                        });
                        context.SaveChanges();
                    }



            }


        }


        private void PrintDb(IHost host, DomainIdentityDbContext context, SqlTransaction trans) {

            _logger.LogInformation($"Dumping ApiResources, Clients, and Users to JSON file ...");

            using var scope = host.Services.CreateScope();

            var cxn = context.Database.GetDbConnection() as SqlConnection;
            var cmd = new SqlCommand("select * from DbView", cxn, trans);
            var result = cmd.ExecuteScalar();
            if (result != null) {

                if (!Directory.Exists(DbViewDir))
                    Directory.CreateDirectory(DbViewDir);

                var path = $"{DbViewDir}\\{DbViewFile}";

                if (File.Exists(path))
                    File.Delete(path);

                using var fs = new FileStream(path, FileMode.CreateNew);
                using var jw = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true, SkipValidation = true });
                JsonDocument.Parse(result.ToString(), default).WriteTo(jw);
            }
        }


        private void MigrateDb(IHost host, string[] args) {

            _logger.LogInformation("Migrating Database...");

            using var scope = host.Services.CreateScope();

            if (args.Contains("/drop"))
                scope.ServiceProvider
                        .GetRequiredService<ConfigurationDbContext>()
                        .Database
                        .EnsureDeleted();

            scope.ServiceProvider
                    .GetRequiredService<ConfigurationDbContext>()
                    .Database
                    .Migrate();
            scope.ServiceProvider
                    .GetRequiredService<PersistedGrantDbContext>()
                    .Database
                    .Migrate();
            scope.ServiceProvider
                    .GetRequiredService<DomainIdentityDbContext>()
                    .Database
                    .Migrate();
        }


        private TContext GetContext<TContext>(SqlConnection cxn, SqlTransaction trans)
            where TContext : DbContext {

            _logger.LogInformation($"Creating {typeof(TContext).Name} instance ...");

            TContext context;

            if (typeof(TContext) == typeof(ConfigurationDbContext)) {
                var options = new DbContextOptionsBuilder<ConfigurationDbContext>()
                    .UseSqlServer(cxn)
                    .EnableSensitiveDataLogging(true)
                    .Options;
                context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options, new ConfigurationStoreOptions() });
            } else if (typeof(TContext) == typeof(PersistedGrantDbContext)) {
                var options = new DbContextOptionsBuilder<PersistedGrantDbContext>()
                    .UseSqlServer(cxn)
                    .EnableSensitiveDataLogging(true)
                    .Options;
                context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options, new OperationalStoreOptions() });
            } else {
                var options = new DbContextOptionsBuilder<DomainIdentityDbContext>()
                    .UseSqlServer(cxn)
                    .EnableSensitiveDataLogging(true)
                    .Options;
                context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options });
            }

            context.Database.UseTransaction(trans);

            return context;
        }




        /// <summary>
        /// per https://stackoverflow.com/a/20622428
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private static string HashPassword(string password) {
            byte[] salt;
            byte[] buffer2;
            if (password == null) {
                throw new ArgumentNullException("password");
            }
            using (var bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8)) {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }




    }
}
