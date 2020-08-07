using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Transactions;


namespace EDennis.AspNetIdentityServer {
    public class Program {

        public const string CONFIGS_DIR = "Configs";
        public const string DUMP_FILE = "DbDump.json";

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

                var configs = Directory.EnumerateFiles(CONFIGS_DIR);


                IEnumerable<string> activeConfigs;
                if (args.Contains("/all"))
                    activeConfigs = configs;
                else
                    activeConfigs = configs.Where(c => args.Contains($"/{c}.json"));

                using var trans = new TransactionScope();
                {
                    LoadConfigs(host, activeConfigs);
                    if (args.Contains("/commit"))
                        trans.Complete();
                    host.Run();

                    if (args.Contains("/dbdump"))
                        DumpDb(host);
                }
            } catch (Exception ex) {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }


        }

        private static void DumpDb(IHost host) {
            using var scope = host.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<DomainIdentityDbContext>();
            var cxn = context.Database.GetDbConnection();
            var cmd = new SqlCommand("select * from DbDump");
            var result = cmd.ExecuteScalar();
            if (result != null) {

                if (!Directory.Exists(CONFIGS_DIR))
                    Directory.CreateDirectory(CONFIGS_DIR);

                var path = $"{CONFIGS_DIR}\\{DUMP_FILE}";

                if (File.Exists(path))
                    File.Delete(path);

                using var fs = new FileStream(path,FileMode.CreateNew);
                using var jw = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true, SkipValidation = true });
                JsonDocument.Parse(result.ToString(), default).WriteTo(jw);
            }
        }


        private static void LoadConfigs(IHost host, IEnumerable<string> activeConfigs) {
            using var scope = host.Services.CreateScope();
            try {
                scope.ServiceProvider
                    .GetRequiredService<PersistedGrantDbContext>()
                    .Database
                    .Migrate();

                var context = scope.ServiceProvider
                    .GetRequiredService<ConfigurationDbContext>();

                //ensure db is migrated before seeding
                context.Database.Migrate();

                //load IdentityResources.json
                var irConfig = new ConfigurationBuilder()
                    .AddJsonFile($"{CONFIGS_DIR}\\IdentityResources.json")
                    .Build();
                var ids = new List<IdentityResource>();
                irConfig.Bind(ids);

                foreach (var resource in ids) {
                    var existing = context.IdentityResources.FirstOrDefault(x => x.Name == resource.Name);
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();


                //load configs for each project
                foreach (var activeConfig in activeConfigs) {

                    var project = activeConfig.Replace(".json", "");

                    var config = new ConfigurationBuilder()
                        .AddJsonFile($"{CONFIGS_DIR}\\{activeConfig}")
                        .Build();

                    //load Api
                    var apis = new List<ApiResource>();
                    config.GetSection("ApiResources").Bind(apis);
                    if (apis.Count == 0)
                        throw new Exception($"Could not find/bind ApiResources section in {CONFIGS_DIR}\\{activeConfig}");

                    foreach (var api in apis) {
                        var existing = context.ApiResources.FirstOrDefault(x => x.Name == api.Name);
                        if (existing != null) {
                            context.ApiResources.Remove(existing);
                            context.SaveChanges();
                        }
                        context.ApiResources.Add(api.ToEntity());
                    }
                    context.SaveChanges();

                    //load clients
                    var clients = new List<ClientConfig>();
                    config.GetSection("Clients").Bind(clients);
                    if (clients.Count == 0)
                        throw new Exception($"Could not find/bind Clients section in {CONFIGS_DIR}\\{activeConfig}");

                    foreach (var client in clients) {
                        var existing = context.Clients.FirstOrDefault(x => x.ClientId == client.ClientId);
                        if (existing != null) {
                            context.Clients.Remove(existing);
                            context.SaveChanges();
                        }
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();


                    //load users
                    var users = new List<TestUser>();
                    config.GetSection("TestUsers").Bind(users);
                    if (users.Count == 0)
                        return;
                    else {
                        LoadUsers(scope, project, users);
                    }
                }

            } catch (Exception ex) {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the Identity database.");
            }

        }

        private static void LoadUsers(IServiceScope scope, string project, List<TestUser> users) {
            
            var _identityDbContext = scope.ServiceProvider
                .GetRequiredService<DomainIdentityDbContext>();
            _identityDbContext.Database.Migrate();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<DomainUser>>();

            Guid appId;
            var app = _identityDbContext.Applications.FirstOrDefault(a => a.Name == project);
            if (app != null)
                appId = app.Id;
            else {
                appId = CombGuid.Create();
                _identityDbContext.Applications.Add(
                    new DomainApplication {
                        Id = appId,
                        Name = project,
                        SysUser = Environment.UserName
                    });
                _identityDbContext.SaveChanges();
            }



            var roles = users.SelectMany(u => u.Roles)
                .ToDictionary(r => r, r => default(Guid));

            foreach (var entry in roles) {
                var role = _identityDbContext.Roles.FirstOrDefault(a =>
                    a.Name == entry.Key && a.ApplicationId == appId);
                if (role != null)
                    roles[entry.Key] = role.Id;
                else {
                    roles[entry.Key] = CombGuid.Create();
                    _identityDbContext.Roles.Add(new DomainRole {
                        Id = roles[entry.Key],
                        ApplicationId = appId,
                        Name = entry.Key,
                        NormalizedName = entry.Key.ToUpper(),
                        SysUser = Environment.UserName
                    });
                    _identityDbContext.SaveChanges();
                }
            }

            foreach (var entry in users) {

                Guid orgId;
                var org = _identityDbContext.Organizations.FirstOrDefault(a => a.Name == entry.OrganizationName);
                if (org != null)
                    orgId = org.Id;
                else {
                    orgId = CombGuid.Create();
                    _identityDbContext.Organizations.Add(
                        new DomainOrganization {
                            Id = orgId,
                            Name = entry.OrganizationName,
                            SysUser = Environment.UserName
                        });
                    _identityDbContext.SaveChanges();
                }



                Guid userId;
                var user = _identityDbContext.Users.FirstOrDefault(a => a.Email == entry.Email);
                if (user != null)
                    userId = user.Id;
                else {
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
                    user.PasswordHash = userManager.PasswordHasher.HashPassword(user, entry.PlainTextPassword);
                    _identityDbContext.Users.Add(user);
                    _identityDbContext.SaveChanges();
                }

                foreach (var role in entry.Roles) {
                    if (!_identityDbContext.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == roles[role])) {
                        _identityDbContext.UserRoles.Add(new DomainUserRole {
                            UserId = userId,
                            RoleId = roles[role],
                            SysUser = Environment.UserName
                        });
                        _identityDbContext.SaveChanges();
                    }
                }

                foreach (var claim in entry.Claims)
                    foreach (var value in claim.Value) {
                        if (!_identityDbContext.UserClaims.Any(ur => ur.UserId == userId && ur.ClaimType == claim.Key && ur.ClaimValue == value)) {
                            _identityDbContext.UserClaims.Add(new DomainUserClaim {
                                UserId = userId,
                                ClaimType = claim.Key,
                                ClaimValue = value,
                                SysUser = Environment.UserName
                            });
                            _identityDbContext.SaveChanges();
                        }
                    }
            }


        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
