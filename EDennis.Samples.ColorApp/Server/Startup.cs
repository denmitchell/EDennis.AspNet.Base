//using EDennis.AspNetIdentityServer;
using EDennis.HostedBlazor.Base;
using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
//using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;

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

            //add integrated IdentityServer
            //see EDennis.AspNetIdentityServer.ICollectionExtensions:
            services.AddIntegratedIdentityServerAndAspNetIdentity<DefaultAppClaimEncoder>(Configuration, 
               "ConnectionStrings:DomainIdentityDbContext");

            /*
            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(
                    Configuration["ConnectionStrings:DomainIdentityDbContext"]));

            services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<DomainIdentityDbContext>();

            services.AddIdentityServer()
                .AddApiAuthorization<DomainUser, PersistedGrantDbContext>()
                .AddConfigurationStore<ConfigurationDbContext>()
                .AddOperationalStore<PersistedGrantDbContext>();
                //.AddConfigurationStore<ConfigurationDbContext>()(options =>
                //{
                //    new DefaultConfigurationStoreOptions().Load(options);
                //})
                //.AddOperationalStore<PersistedGrantDbContext>(options =>
                //{
                //    new DefaultOperationalStoreOptions().Load(options);
                //});

            services.AddAuthentication()
                .AddIdentityServerJwt();

            */

            services.AddControllersWithViews(options=>
                //add default policies that allow pattern matching on scopes
                options.AddDefaultPolicies<Startup>(services, HostEnvironment, Configuration));
            
            services.AddRazorPages();

            //for generating the OAuth Access Token
            //services.AddSecureTokenService<MockTokenService>(Configuration);

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

            app.UseMockClaimsPrincipalFor("/Rgb");
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseClaimsToHeaderFor("/Rgb");
            app.UseAuthorization();
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
