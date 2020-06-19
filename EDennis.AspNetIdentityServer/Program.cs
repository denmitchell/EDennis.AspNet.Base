using EDennis.AspNetIdentityServer.Data;
using EDennis.AspNetIdentityServer.Models;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Linq;
using System.Security.Claims;

namespace EDennis.AspNetIdentityServer {
    public class Program {
        public static int Main(string[] args) {

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

                //seed the database if /seed argument provided
                if (args.Contains("/seed")) {
                    CreateAndSeedUserDatabase(host);
                    CreateAndSeedIdentityDatabase(host);
                    args = args.Except(new[] { "/seed" }).ToArray();
                }

                //run the app
                host.Run();
            } catch (Exception ex) {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }

            return 0;



        }

        private static void CreateAndSeedIdentityDatabase(IHost host) {
            //seed the database
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

                if (!context.Clients.Any()) {
                    foreach (var client in Config.Clients) {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any()) {
                    foreach (var resource in Config.Ids) {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any()) {
                    foreach (var resource in Config.Apis) {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }


            } catch (Exception ex) {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the Identity database.");
            }
        }



        private static void CreateAndSeedUserDatabase(IHost host) {
            //seed the database
            using var scope = host.Services.CreateScope();
            try {

                var context = scope.ServiceProvider.GetService<AspNetIdentityDbContext>();

                //ensure db is migrated before seeding
                context.Database.Migrate();

                var anyRecs = context.Users.Any();
                if (anyRecs)
                    return;


                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var admin = new IdentityRole { Name = "MvcApp.Admin" };
                var user = new IdentityRole { Name = "MvcApp.User" };
                var readOnly = new IdentityRole { Name = "MvcApp.Readonly" };


                var admin2 = new IdentityRole { Name = "BlazorApp3.Admin" };
                var user2 = new IdentityRole { Name = "BlazorApp3.User" };
                var readOnly2 = new IdentityRole { Name = "BlazorApp3.Readonly" };


                roleManager.CreateAsync(admin).Wait();
                roleManager.CreateAsync(user).Wait();
                roleManager.CreateAsync(readOnly).Wait();

                roleManager.CreateAsync(admin2).Wait();
                roleManager.CreateAsync(user2).Wait();
                roleManager.CreateAsync(readOnly2).Wait();



                context.SaveChanges();

                roleManager.AddClaimAsync(admin, new Claim("user_scope", "Api1.*.Get*")).Wait();
                roleManager.AddClaimAsync(admin, new Claim("user_scope", "Api1.*.Edit*")).Wait();
                roleManager.AddClaimAsync(admin, new Claim("user_scope", "Api1.*.Delete*")).Wait();
                roleManager.AddClaimAsync(user, new Claim("user_scope", "Api1.*.Get*")).Wait();
                roleManager.AddClaimAsync(user, new Claim("user_scope", "Api1.*.Edit*")).Wait();
                roleManager.AddClaimAsync(readOnly, new Claim("user_scope", "Api1.*.Get*")).Wait();

                roleManager.AddClaimAsync(admin2, new Claim("user_scope", "Api1.*.Get*")).Wait();
                roleManager.AddClaimAsync(admin2, new Claim("user_scope", "Api1.*.Edit*")).Wait();
                roleManager.AddClaimAsync(admin2, new Claim("user_scope", "Api1.*.Delete*")).Wait();
                roleManager.AddClaimAsync(user2, new Claim("user_scope", "Api1.*.Get*")).Wait();
                roleManager.AddClaimAsync(user2, new Claim("user_scope", "Api1.*.Edit*")).Wait();
                roleManager.AddClaimAsync(readOnly2, new Claim("user_scope", "Api1.*.Get*")).Wait();


                roleManager.AddClaimAsync(admin2, new Claim("user_scope", "Api2.*.Get*")).Wait();
                roleManager.AddClaimAsync(admin2, new Claim("user_scope", "Api2.*.Edit*")).Wait();
                roleManager.AddClaimAsync(admin2, new Claim("user_scope", "Api2.*.Delete*")).Wait();
                roleManager.AddClaimAsync(user2, new Claim("user_scope", "Api2.*.Get*")).Wait();
                roleManager.AddClaimAsync(user2, new Claim("user_scope", "Api2.*.Edit*")).Wait();
                roleManager.AddClaimAsync(readOnly2, new Claim("user_scope", "Api2.*.Get*")).Wait();


                context.SaveChanges();


                //use the user manager to create test users
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AspNetIdentityUser>>();

                var moe = new AspNetIdentityUser { UserName = "Moe@stooges.org", Email="moe@stooges.org", EmailConfirmed = true};
                var larry = new AspNetIdentityUser { UserName = "Larry@stooges.org", Email = "larry@stooges.org", EmailConfirmed = true };
                var curly = new AspNetIdentityUser { UserName = "Curly@stooges.org", Email = "curly@stooges.org", EmailConfirmed = true };

                userManager.CreateAsync(moe, "P@ssword1").Wait();
                userManager.CreateAsync(larry, "P@ssword1").Wait();
                userManager.CreateAsync(curly, "P@ssword1").Wait();

                context.SaveChanges();

                userManager.AddToRoleAsync(moe, "MvcApp.Admin").Wait();
                userManager.AddToRoleAsync(larry, "MvcApp.User").Wait();
                userManager.AddToRoleAsync(curly, "MvcApp.Readonly").Wait();

                userManager.AddToRoleAsync(moe, "BlazorApp3.Readonly").Wait();
                userManager.AddToRoleAsync(larry, "BlazorApp3.Admin").Wait();
                userManager.AddToRoleAsync(curly, "BlazorApp3.User").Wait();


                context.SaveChanges();

                userManager.AddClaimsAsync(moe, new Claim[] {
                                new Claim(JwtClaimTypes.Name, "Moe"),
                                new Claim(JwtClaimTypes.Email, "moe@stooges.org")
                            }).Wait();

                userManager.AddClaimsAsync(larry, new Claim[] {
                                new Claim(JwtClaimTypes.Name, "Larry"),
                                new Claim(JwtClaimTypes.Email, "larry@stooges.org")
                            }).Wait();

                userManager.AddClaimsAsync(curly, new Claim[] {
                                new Claim(JwtClaimTypes.Name, "Curly"),
                                new Claim(JwtClaimTypes.Email, "curly@stooges.org")
                            }).Wait();

                context.SaveChanges();


            } catch (Exception ex) {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while seeding the User database.");
            }
        }


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
