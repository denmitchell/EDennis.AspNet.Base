using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace EDennis.AspNetIdentityServer {

    public class Program {

        public const string CONFIGS_DIR = "Configs";
        public const string DBVIEW_DIR = "Logs";
        public const string DBVIEW_FILE = "DbView.json";
        public static Regex FILE_PROJECT_EXTRACTOR = new Regex(@"(?<=Configs\\)([A-Za-z0-9_.]+)(?=\.json)");
        public const string IDENTITY_RESOURCES_FILE = "IdentityResources.json";


        public static void Main(string[] args) {

            //Debugger.Launch();

            Log.Logger = new LoggerConfiguration()
                .GetLoggerFromConfiguration<Program>("Logging:Serilog");

            try {
                Log.Information("Starting host...");
                var host = CreateHostBuilder(args).Build();

                if (args.Contains("/migrate"))
                    MigrateDb(host, args);

                var configs = Directory.EnumerateFiles(CONFIGS_DIR);

                IEnumerable<string> projects;
                if (args.Contains("/all"))
                    projects = configs.Where(c => c != $"{CONFIGS_DIR}\\{IDENTITY_RESOURCES_FILE}")
                        .Select(c => FILE_PROJECT_EXTRACTOR.Match(c).Value);
                else
                    projects = args.Select(a => a[1..]).Where(a => configs.Contains($"{CONFIGS_DIR}\\{a}.json"));

                var print = args.Contains("/print");
                var commit = args.Contains("/commit");


                if (projects.Count() == 0)
                    host.Run();
                else
                    RunWithConfigs(host, projects, print, commit);

            } catch (Exception ex) {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }


        }

        private static void RunWithConfigs(IHost host, IEnumerable<string> projects, bool print, bool commit) {

            using var scope = host.Services.CreateScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var cxnString = config["DbContexts:AspNetIdentityDbContext:ConnectionString"];

            using var cxn = new SqlConnection(cxnString);
            cxn.Open();
            using var trans = cxn.BeginTransaction();

            var diContext = GetContext<DomainIdentityDbContext>(cxn, trans);
            var cContext = GetContext<ConfigurationDbContext>(cxn, trans);
            var pContext = GetContext<PersistedGrantDbContext>(cxn, trans);

            LoadConfigs(projects, cContext, diContext);

            Log.Information("**Finished Loading Configurations.");

            if (commit)
                trans.Commit();

            if (print)
                PrintDb(host, diContext, trans);

            host.Run();

            if (!commit)
                trans.Rollback();


        }

        private static void MigrateDb(IHost host, string[] args) {

            Log.Information("Migrating Database...");

            using var scope = host.Services.CreateScope();

            if(args.Contains("/drop"))
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

        private static TContext GetContext<TContext>(SqlConnection cxn, SqlTransaction trans)
            where TContext : DbContext {

            Log.Information($"Creating {typeof(TContext).Name} instance ...");

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





        private static void PrintDb(IHost host, DomainIdentityDbContext context, SqlTransaction trans) {

            Log.Information($"Dumping ApiResources, Clients, and Users to JSON file ...");

            using var scope = host.Services.CreateScope();

            var cxn = context.Database.GetDbConnection() as SqlConnection;
            var cmd = new SqlCommand("select * from DbView", cxn, trans);
            var result = cmd.ExecuteScalar();
            if (result != null) {

                if (!Directory.Exists(DBVIEW_DIR))
                    Directory.CreateDirectory(DBVIEW_DIR);

                var path = $"{DBVIEW_DIR}\\{DBVIEW_FILE}";

                if (File.Exists(path))
                    File.Delete(path);

                using var fs = new FileStream(path, FileMode.CreateNew);
                using var jw = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true, SkipValidation = true });
                JsonDocument.Parse(result.ToString(), default).WriteTo(jw);
            }
        }


        private static void LoadConfigs(IEnumerable<string> projects,
            ConfigurationDbContext cContext, DomainIdentityDbContext diContext) {

            Log.Information($"Loading {CONFIGS_DIR}\\{IDENTITY_RESOURCES_FILE} ...");

            var irConfig = new ConfigurationBuilder()
                    .AddJsonFile($"{CONFIGS_DIR}\\{IDENTITY_RESOURCES_FILE}")
                    .Build();
            var ids = new List<IdentityResource>();
            irConfig.GetSection("IdentityResources").Bind(ids);

            if (ids.Count == 0)
                throw new Exception($"Could not find/bind {CONFIGS_DIR}\\{IDENTITY_RESOURCES_FILE}");


            foreach (var resource in ids) {
                var existing = cContext.IdentityResources.FirstOrDefault(x => x.Name == resource.Name);
                if (existing == null) {
                    Log.Information($"\tAdding IdentityResource {resource.Name}");
                    var entity = resource.ToEntity();
                    cContext.IdentityResources.Add(entity);
                }
            }
            cContext.SaveChanges();


            Log.Information($"Loading project configs from {CONFIGS_DIR} ...");

            foreach (var project in projects) {

                var config = new ConfigurationBuilder()
                    .AddJsonFile($"{CONFIGS_DIR}\\{project}.json")
                    .Build();

                Log.Information($"\tLoading Apis for {project} ...");
                var apis = new List<ApiResource>();
                config.GetSection("ApiResources").Bind(apis);

                if (apis.Count == 0)
                    throw new Exception($"Could not find/bind ApiResources section in {CONFIGS_DIR}\\{project}");

                foreach (var api in apis) {
                    Log.Information($"\t\tLoading {api.Name} ...");
                    var existing = cContext.ApiResources.FirstOrDefault(x => x.Name == api.Name);
                    if (existing != null) {
                        cContext.ApiResources.Remove(existing);
                        cContext.SaveChanges();
                    }
                    cContext.ApiResources.Add(api.ToEntity());
                }
                cContext.SaveChanges();

                Log.Information($"\tLoading Clients for {project} ...");
                var clients = new List<ClientConfig>();
                config.GetSection("Clients").Bind(clients);
                if (clients.Count == 0)
                    throw new Exception($"Could not find/bind Clients section in {CONFIGS_DIR}\\{project}");

                foreach (var client in clients) {
                    Log.Information($"\t\tLoading {client.ClientId} ...");
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
                    Log.Information($"\tNo TestUsers found for {project} ...");
                    return;
                } else {
                    Log.Information($"\tLoading TestUsers for {project} ...");
                    LoadTestUsers(diContext, project, users);
                }
            }

        }

        private static void LoadTestUsers(DomainIdentityDbContext context, string project, List<TestUser> users) {

            Log.Information($"\t\tChecking application record for {project} ...");
            var app = context.Set<DomainApplication>().FirstOrDefault(a => a.Name == project);
            if (app != null) {
                Log.Information($"\t\tApplication record found for {project} ...");
            } else {
                Log.Information($"\t\tAdding application record for {project} ...");
                app = new DomainApplication {
                    Name = project
                };
                context.Set<DomainApplication>().Add(app);
                context.SaveChanges();
            }



            Log.Information($"\t\tAdding any missing application claim records ...");

            var roles = users.SelectMany(u => u.Roles)
                .ToDictionary(r => r, r => default(int));

            var applicationClaimRecords = context.ApplicationClaims
                .Where(ac => ac.ClaimType == "app:role" && ac.ClaimValue.StartsWith($"{project}:"))
                .ToList();

            var missingApplicationClaims = roles.Keys
                .Where(r => !applicationClaimRecords
                .Any(ac => ac.ClaimValue.EndsWith($":{r}")))
                .Select(r => new DomainApplicationClaim { 
                    Application = project, 
                    ClaimType = "app:role", 
                    ClaimValue = r, 
                    OrgAdminable = true 
                });

            context.ApplicationClaims.AddRange(missingApplicationClaims);
            context.SaveChanges();



            Log.Information($"\t\t\tAdding any missing organization records ...");

            var orgs = users.Select(u => u.Organization).Distinct();

            var existingOrgs = context.Set<DomainOrganization>()
                .Select(o => o.Name).ToList();

            var missingOrgs = orgs.Except(existingOrgs)
                .Select(o => new DomainOrganization { Name = o });

            context.Organizations.AddRange(missingOrgs);
            context.SaveChanges();



            Log.Information($"\t\t\tAdding any missing organization application records ...");

            var existingOrgApps = context.OrganizationApplications
                .Where(oa => oa.Application == project)
                .Select(oa=> oa.Organization);

            var missingOrgApps = orgs.Except(existingOrgApps)
                .Select(o => new DomainOrganizationApplication { Organization = o, Application = project });

            context.OrganizationApplications.AddRange(missingOrgApps);
            context.SaveChanges();



            Log.Information($"\t\tChecking test user records for {project} ...");

            foreach (var entry in users) {


                Log.Information($"\t\t\tChecking user record for {entry.Email} ...");

                int userId = default;
                var user = context.Users.FirstOrDefault(a => a.Email == entry.Email);
                if (user != null) {
                    userId = user.Id;
                    Log.Information($"\t\t\tUser record found for {entry.Email} ...");
                } else {
                    Log.Information($"\t\t\tAdding user record for {entry.Email} ...");
                    user = new DomainUser {
                        Email = entry.Email,
                        NormalizedEmail = entry.Email,
                        UserName = entry.Email,
                        NormalizedUserName = entry.Email,
                        EmailConfirmed = true,
                        PhoneNumber = entry.PhoneNumber,
                        PhoneNumberConfirmed = true,
                        Organization = entry.Organization,
                        OrganizationConfirmed = true,
                        OrganizationAdmin = entry.OrganizationAdmin
                    };
                    user.PasswordHash = HashPassword(entry.PlainTextPassword);
                    context.Users.Add(user);
                    context.SaveChanges();
                    userId = user.Id;
                }


                Log.Information($"\t\t\tChecking claim records for {entry.Email} ...");

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
            }


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


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>                
                {
                    webBuilder.UseSerilog();
                    webBuilder.UseStartup<Startup>();
                });
    }

}
