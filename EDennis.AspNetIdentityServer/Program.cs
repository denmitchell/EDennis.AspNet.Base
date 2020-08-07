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
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Transactions;


namespace EDennis.AspNetIdentityServer {
    public class Program {

        public const string CONFIGS_DIR = "Configs";
        public const string DUMP_FILE = "DbDump.json";
        public static Regex FILE_PROJECT_EXTRACTOR = new Regex(@"(?<=Configs\\)([A-Za-z0-9_.]+)(?=\.json)");
        public const string IDENTITY_RESOURCES_FILE = "IdentityResources.json";


        public static void Main(string[] args) {

            //Debugger.Launch();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            try {
                Log.Information("Starting host...");
                var host = CreateHostBuilder(args).Build();

                if (args.Contains("/migrate"))
                    MigrateDb(host);

                var configs = Directory.EnumerateFiles(CONFIGS_DIR);

                IEnumerable<string> projects;
                if (args.Contains("/all"))
                    projects = configs.Where(c => c != $"{CONFIGS_DIR}\\{IDENTITY_RESOURCES_FILE}")
                        .Select(c => FILE_PROJECT_EXTRACTOR.Match(c).Value);
                else
                    projects = args.Select(a => a[1..]).Where(a => configs.Contains($"{CONFIGS_DIR}\\{a}.json"));

                var dump = args.Contains("/dump");
                var commit = args.Contains("/commit");


                if (projects.Count() == 0)
                    host.Run();
                else
                    RunWithConfigs(host, projects, dump, commit);

            } catch (Exception ex) {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }


        }

        private static void RunWithConfigs(IHost host, IEnumerable<string> projects, bool dump, bool commit) {

            using var scope = host.Services.CreateScope();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var cxnString = config["DbContexts:AspNetIdentityDbContext:ConnectionString"];

            using var cxn = new SqlConnection(cxnString);
            cxn.Open();
            using var trans = cxn.BeginTransaction();

            var diContext = GetContext<DomainIdentityDbContext>(cxn, trans);
            var cContext = GetContext<ConfigurationDbContext>(cxn, trans);
            var pContext = GetContext<PersistedGrantDbContext>(cxn, trans);

            LoadConfigs(host, projects, cContext, diContext);

            if (commit)
                trans.Commit();

            host.Run();

            if (dump)
                DumpDb(host, diContext);

            if (!commit)
                trans.Rollback();


        }

        private static void MigrateDb(IHost host) {

            Log.Information("Migrating Database...");

            using var scope = host.Services.CreateScope();
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
                    .Options;
                context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options, new ConfigurationStoreOptions() });
            } else if (typeof(TContext) == typeof(PersistedGrantDbContext)) {
                var options = new DbContextOptionsBuilder<PersistedGrantDbContext>()
                    .UseSqlServer(cxn)
                    .Options;
                context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options, new OperationalStoreOptions() });
            } else {
                var options = new DbContextOptionsBuilder<DomainIdentityDbContext>()
                    .UseSqlServer(cxn)
                    .Options;
                context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options });
            }

            context.Database.UseTransaction(trans);

            return context;
        }





        private static void DumpDb(IHost host, DomainIdentityDbContext context) {

            Log.Information($"Dumping ApiResources, Clients, and Users to JSON file ...");

            using var scope = host.Services.CreateScope();

            var cxn = context.Database.GetDbConnection();
            var cmd = new SqlCommand("select * from DbDump");
            var result = cmd.ExecuteScalar();
            if (result != null) {

                if (!Directory.Exists(CONFIGS_DIR))
                    Directory.CreateDirectory(CONFIGS_DIR);

                var path = $"{CONFIGS_DIR}\\{DUMP_FILE}";

                if (File.Exists(path))
                    File.Delete(path);

                using var fs = new FileStream(path, FileMode.CreateNew);
                using var jw = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true, SkipValidation = true });
                JsonDocument.Parse(result.ToString(), default).WriteTo(jw);
            }
        }


        private static void LoadConfigs(IHost host, IEnumerable<string> projects,
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
                    LoadUsers(diContext, project, users);
                }
            }

        }

        private static void LoadUsers(DomainIdentityDbContext context, string project, List<TestUser> users) {

            Log.Information($"\t\tChecking application record for {project} ...");
            Guid appId;
            var app = context.Applications.FirstOrDefault(a => a.Name == project);
            if (app != null) {
                Log.Information($"\t\tApplication record found for {project} ...");
                appId = app.Id;
            } else {
                Log.Information($"\t\tAdding application record for {project} ...");
                appId = CombGuid.Create();
                context.Applications.Add(
                    new DomainApplication {
                        Id = appId,
                        Name = project,
                        SysUser = Environment.UserName
                    });
                context.SaveChanges();
            }



            Log.Information($"\t\tChecking role records for {project} ...");
            var roles = users.SelectMany(u => u.Roles)
                .ToDictionary(r => r, r => default(Guid));

            foreach (var entry in roles) {
                var role = context.Roles.FirstOrDefault(a =>
                    a.Name == entry.Key && a.ApplicationId == appId);
                if (role != null) {
                    Log.Information($"\t\tRole {entry.Key} record found for {project} ...");
                    roles[entry.Key] = role.Id;
                } else {
                    Log.Information($"\t\tAdding role {entry.Key} record for {project} ...");
                    roles[entry.Key] = CombGuid.Create();
                    context.Roles.Add(new DomainRole {
                        Id = roles[entry.Key],
                        ApplicationId = appId,
                        Name = entry.Key,
                        NormalizedName = entry.Key.ToUpper(),
                        SysUser = Environment.UserName
                    });
                    context.SaveChanges();
                }
            }


            Log.Information($"\t\tChecking test user records for {project} ...");

            foreach (var entry in users) {

                Log.Information($"\t\t\tChecking organization record for {entry.Email} ...");

                Guid orgId;
                var org = context.Organizations.FirstOrDefault(a => a.Name == entry.OrganizationName);
                if (org != null) {
                    Log.Information($"\t\t\tOrganization {entry.OrganizationName} record found for {entry.Email} ...");
                    orgId = org.Id;
                } else {
                    Log.Information($"\t\t\tAdding Organization {entry.OrganizationName} record for {entry.Email} ...");
                    orgId = CombGuid.Create();
                    context.Organizations.Add(
                        new DomainOrganization {
                            Id = orgId,
                            Name = entry.OrganizationName,
                            SysUser = Environment.UserName
                        });
                    context.SaveChanges();
                }



                Log.Information($"\t\t\tChecking user record for {entry.Email} ...");

                Guid userId;
                var user = context.Users.FirstOrDefault(a => a.Email == entry.Email);
                if (user != null) {
                    userId = user.Id;
                    Log.Information($"\t\t\tUser record found for {entry.Email} ...");
                } else {
                    Log.Information($"\t\t\tAdding user record for {entry.Email} ...");
                    userId = CombGuid.Create();
                    user = new DomainUser {
                        Id = userId,
                        Email = entry.Email,
                        NormalizedEmail = entry.Email.ToUpper(),
                        UserName = entry.Email,
                        NormalizedUserName = entry.Email.ToUpper(),
                        EmailConfirmed = true,
                        SysUser = Environment.UserName
                    };
                    user.PasswordHash = HashPassword(entry.PlainTextPassword);
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                Log.Information($"\t\t\tChecking user role records for {entry.Email} ...");

                foreach (var role in entry.Roles) {
                    if (!context.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == roles[role])) {
                        context.UserRoles.Add(new DomainUserRole {
                            UserId = userId,
                            RoleId = roles[role],
                            SysUser = Environment.UserName
                        });
                        context.SaveChanges();
                    }
                }

                Log.Information($"\t\t\tChecking claim records for {entry.Email} ...");

                foreach (var claim in entry.Claims)
                    foreach (var value in claim.Value) {
                        if (!context.UserClaims.Any(ur => ur.UserId == userId && ur.ClaimType == claim.Key && ur.ClaimValue == value)) {
                            context.UserClaims.Add(new DomainUserClaim {
                                UserId = userId,
                                ClaimType = claim.Key,
                                ClaimValue = value,
                                SysUser = Environment.UserName
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
                    webBuilder.UseStartup<Startup>();
                });
    }

}
