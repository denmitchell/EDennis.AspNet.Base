using EDennis.AspNet.Base;
using EDennis.AspNetIdentityServer.Data;
using EDennis.AspNetIdentityServer.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EDennis.AspNetIdentityServer {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            //Debugger.Launch();

            string cxnAspNetIdentity = Configuration["DbContexts:AspNetIdentityDbContext:ConnectionString"];
            services.AddDbContext<AspNetIdentityDbContext>(options =>
                options.UseSqlServer(cxnAspNetIdentity));

            //services.AddDefaultIdentity<AspNetIdentityUser>(options => {
            //    options.SignIn.RequireConfirmedAccount = true;
            //    options.ClaimsIdentity.RoleClaimType = "role";
            //    options.ClaimsIdentity.UserIdClaimType = "sub";
            //    options.ClaimsIdentity.UserNameClaimType = "name";
            //})
            //   .AddRoles<IdentityRole>()
            //   .AddEntityFrameworkStores<AspNetIdentityDbContext>();


            services.AddIdentity<AspNetIdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                           .AddEntityFrameworkStores<AspNetIdentityDbContext>()
                           .AddDefaultTokenProviders();

            services.AddControllersWithViews();
            services.AddRazorPages();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string cxnConfiguration = Configuration["DbContexts:ConfigurationDbContext:ConnectionString"];
            string cxnPersistedGrant = Configuration["DbContexts:PersistedGrantDbContext:ConnectionString"];

            services.AddTransient<IEmailSender, MockEmailSender>();

            services.AddIdentityServer()
                .AddConfigurationStore(options => {
                    options.ConfigureDbContext = b => b.UseSqlServer(cxnConfiguration,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = b => b.UseSqlServer(cxnPersistedGrant,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddDeveloperSigningCredential()
                .AddAspNetIdentity<AspNetIdentityUser>()
                .AddProfileService<UserClientClaimsProfileService<AspNetIdentityDbContext, AspNetIdentityUser>>(); ;

            //replace Identity Server's ProfileService with a profile service that determines
            //which claims to retrieve for a user/client as configured in the database
            services.Replace(ServiceDescriptor.Transient<IProfileService, UserClientClaimsProfileService<AspNetIdentityDbContext, AspNetIdentityUser>> ());

            services.AddOidcLogging(Configuration);

            //services.AddAuthentication()
            //    .AddGoogle("Google", options => {
            //        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //        options.ClientId = "<insert here>";
            //        options.ClientSecret = "<insert here>";
            //    })
            //    .AddMicrosoftAccount("Microsoft", options => {
            //        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //        options.ClientId = "<insert here>";
            //        options.ClientSecret = "<insert here>";
            //    });
                
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger) {

            app.UseOidcLogging();


            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            //app.UseAuthentication(); //UseIdentityServer calls this
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }



    }
}
