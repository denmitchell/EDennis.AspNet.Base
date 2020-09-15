using EDennis.NetApp.Base;
using EDennis.NetStandard.Base;
using EDennis.Samples.ColorApp.Client;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
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

            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<DomainIdentityDbContext>();

            services.AddIdentityServer()
                /*
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                */                
                .AddApiAuthorization<DomainUser, PersistedGrantDbContext>();


            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options => {
            //        options.Authority = "https://localhost:5000";
            //        options.Audience = "EDennis.Samples.ColorApp.Server";
            //        options.TokenValidationParameters = new TokenValidationParameters() {
            //            NameClaimType = "name",
            //            RoleClaimType = "role:EDennis.Samples.ColorApp.Server"
            //        };
            //    });

            services.AddAuthentication()
                .AddIdentityServerJwt();


            services.AddControllersWithViews(options=>
                //add default policies that allow pattern matching on scopes
                options.AddDefaultPolicies<Startup>(services, HostEnvironment, Configuration));

            services.AddAuthorization(options => {
                IServiceCollectionExtensions_DefaultPolicies.LoadDefaultPolicies<Startup>(options, new List<string> { "scope" });
            });


            services.AddRazorPages();

            services.AddHttpLogging(Configuration);


            services.AddProxyClient(Configuration,"RgbClient");


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

            //for interactively testing the APIs directly
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ColorApi (via Blazor App)", Version = "v1" });
            });

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
            app.UseClaimsToHeaderFor("/Rgb");
            app.UseCachedTransactionCookieFor("/Rgb");
            app.UseScopedRequestMessageFor("/Rgb");


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ColorApi Via Blazor App");
            });

        }
    }
}
