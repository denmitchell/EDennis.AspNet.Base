using EDennis.NetStandard.Base;
using EDennis.Samples.ColorApp.Server.Models;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EDennis.Samples.ColorApp.Server {
    public class Startup {
        public Startup(IConfiguration configuration, IHostEnvironment env) {
            Configuration = configuration;
            HostEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {



            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var connectionString = Configuration.GetValueOrThrow<string>("ConnectionStrings:DomainIdentityDbContext");

            services.AddDbContext<IntegratedDomainIdentityDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<IntegratedDomainIdentityDbContext>();



            services.AddSingleton<IClientRequestParametersProvider>(new ClientRequestParametersProvider(
                "https://localhost:5000",
                "EDennis.Samples.ColorApp.Client",
                "https://localhost:44336/authentication/login-callback",/*"https://localhost:44336/signin-oidc",*/
                "https://localhost:44336/",/*"https://localhost:44336/signout-callback-oidc",*/
                "code",
                "openid profile EDennis.Samples.ColorApp.ServerAPI"
                ));



            //the current Microsoft templates are not compatible with IdentityServer4.x
            //https://github.com/IdentityServer/IdentityServer4/issues/4752
            services.AddIdentityServer()
                .AddAspNetIdentity<DomainUser>()
                .AddDeveloperSigningCredential()
                .AddInMemoryPersistedGrants()
                .AddInMemoryIdentityResources(new List<IdentityResource> {
                    new IdentityResource {
                        Name="openid",
                        UserClaims = new string[] {
                            "sub"
                        }
                    },
                    new IdentityResource {
                        Name = "profile",
                        Description = "User Profile",
                        UserClaims = new string[] {
                            "name",
                            "email",
                            "organization",
                            "organization_confirmed",
                            "organization_admin_for",
                            "super_admin",
                            "role:EDennis.Samples.ColorApp.Server"
                        }
                    }
                })
                .AddInMemoryApiResources(new List<ApiResource>() {
                    new ApiResource {
                        Name = "EDennis.Samples.ColorApp.ServerAPI",
                        Scopes = { "EDennis.Samples.ColorApp.ServerAPI" }
                    }
                })
                .AddInMemoryApiScopes(new List<ApiScope>() {
                    new ApiScope {
                        Name = "EDennis.Samples.ColorApp.ServerAPI"
                    }
                })
                .AddInMemoryClients(new List<IdentityServer4.Models.Client> {
                    new IdentityServer4.Models.Client {
                        ClientId="EDennis.Samples.ColorApp.Client",
                        AlwaysIncludeUserClaimsInIdToken=true,
                        AllowedGrantTypes={ GrantType.AuthorizationCode },
                        RedirectUris={ "https://localhost:44336/signin-oidc" },
                        PostLogoutRedirectUris= { "https://localhost:44336/signout-callback-oidc" },
                        //RequirePkce = true,
                        AllowedScopes ={"openid","profile","EDennis.Samples.ColorApp.ServerAPI"}
                    }
                });
                /*
                .AddApiAuthorization<DomainUser, IntegratedDomainIdentityDbContext>(options => {
                    var profile = options.IdentityResources.FirstOrDefault(x => x.Name == "profile");
                    if (profile != null)
                        options.IdentityResources.Remove(profile);

                    profile = new IdentityResource { Name = "profile", Description = "User Profile" };
                    profile.UserClaims.Clear();
                    profile.UserClaims.Add("name");
                    profile.UserClaims.Add("email");
                    profile.UserClaims.Add("organization");
                    profile.UserClaims.Add("organization_confirmed");
                    profile.UserClaims.Add("organization_admin_for");
                    profile.UserClaims.Add("super_admin");
                    profile.UserClaims.Add("role:EDennis.Samples.ColorApp.Server");
                    options.IdentityResources.Add(profile);
                });
                */

            services.AddAuthentication()
                .AddIdentityServerJwt();


            services.AddControllersWithViews(options => {
                //add default policies that allow pattern matching on scopes
                //options.AddDefaultPolicies<Startup>(services, HostEnvironment, Configuration);
            });

            services.AddAuthorization(options => {
                //IServiceCollectionExtensions_DefaultPolicies.LoadDefaultPolicies<Startup>(options, new List<string> { "scope" });
            });


            services.AddRazorPages();

            services.AddHttpLogging(Configuration);


            services.AddProxyClient(Configuration, "RgbClient");


            //for generating the OAuth Access Token
            services.AddSecureTokenService<MockTokenService>(Configuration);

            //for propagating headers and cookies to child API (ColorApi)
            services.AddScopedRequestMessage(Configuration);

            //for mocking the user/client
            services.AddMockClaimsPrincipal(Configuration);

            //for propagating user claims to the child API (ColorApi) via headers
            services.AddClaimsToHeader(Configuration);

            //for creating a cookie that holds the database transaction key
            services.AddCachedTransactionCookie(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseWebAssemblyDebugging();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseHttpLogging();
            //app.UseMockClaimsPrincipalFor("/Rgb");
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.Use(async(context, next) => {

                await next();
            });
            app.UseClaimsToHeaderFor("/Rgb");
            app.UseCachedTransactionCookieFor("/Rgb");
            app.UseScopedRequestMessageFor("/Rgb");


            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

        }
    }
}
