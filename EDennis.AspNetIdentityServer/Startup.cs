using EDennis.AspNetIdentityServer;
using EDennis.MigrationsExtensions;
using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Reflection;

namespace EDennis.AspNetIdentityServer {
    public class Startup {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {

            //Debugger.Launch();

            services.AddControllersWithViews();
            services.AddRazorPages();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            string cxnConfiguration = Configuration["DbContexts:ConfigurationDbContext:ConnectionString"];
            string cxnPersistedGrant = Configuration["DbContexts:PersistedGrantDbContext:ConnectionString"];
            string cxnAspNetIdentity = Configuration["DbContexts:AspNetIdentityDbContext:ConnectionString"];

            services.AddScoped<IUserClaimsPrincipalFactory<DomainUser>, DomainUserClaimsPrincipalFactory>();

            services.AddIdentityServer()
                .AddConfigurationStore(options => {
                    new DefaultConfigurationStoreOptions().Load(options);
                    options.ConfigureDbContext = b => b.UseSqlServer(cxnConfiguration,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options => {
                    new DefaultOperationalStoreOptions().Load(options);
                    options.ConfigureDbContext = b => b.UseSqlServer(cxnPersistedGrant,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddDeveloperSigningCredential()
                .AddAspNetIdentity<DomainUser>()
                .AddProfileService<DomainIdentityProfileService>();

            services.AddDbContext<DomainIdentityDbContext>(options =>
                    options.UseSqlServer(cxnAspNetIdentity,
                        sql => sql.MigrationsAssembly(migrationsAssembly))
                    .ReplaceService<IMigrationsSqlGenerator,MigrationsExtensionsSqlGenerator>());


            services.AddIdentity<DomainUser, DomainRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<DomainIdentityDbContext>()
                //.AddUserManager<>
                //.AddRoleManager<>
                .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, MockEmailSender>();

            //replace Identity Server's ProfileService with a profile service that determines
            //which claims to retrieve for a user/client as configured in the database
            services.Replace(ServiceDescriptor.Transient<IProfileService, DomainIdentityProfileService>());

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


            services.AddAuthorization(options =>
            {
                var settings = new AdministrationSettings();
                Configuration.GetSection("Administration").Bind(settings);
                if(settings.PolicyType == PolicyType.Unconfigured && settings.PolicyName == default)
                    throw new System.Exception("IdentityServer configuration does not contain an Administration section that can bind to an AdministrationSettings object.");

                options.AddPolicy(settings.PolicyName, policy =>
                {
                    if (settings.PolicyType == PolicyType.Open)
                        policy.RequireAssertion(context => true); //open
                    else if (settings.PolicyType == PolicyType.Role)
                        policy.RequireRole(settings.RoleName);
                });
            });

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("IdentityServer", new OpenApiInfo { Title = "IdentityServer", Version = "v4" });
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {

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


            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityServer V4");
            });

        }



    }
}
