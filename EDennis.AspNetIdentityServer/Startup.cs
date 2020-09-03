using EDennis.NetApp.Base;
using EDennis.NetStandard.Base;
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
using Microsoft.OpenApi.Models;
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

            services.AddRazorPages(options => {
                options.Conventions.AuthorizeFolder("/Account/Admin");
            });

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;


            //get connection strings from configuration;
            //special note: when using Launcher, configurations are propagated via command-line arguments. Because
            //   connection string configuration values have embedded "=", the entire connection string is quoted;
            //   accordingly, you need to remove those quotes before processing.  The GetValueOrThrow extension
            //   method can remove these quotes for you.  (An alternative is to break up the connection string
            //   into its constituent parts in configuration -- e.g. have separate configuration settings for 
            //   Server, Database, Trusted_Connection, and MultipleActiveResultSets; bind these to an
            //   EDennis.NetStandard.Base.ConnectionString instance; and retrieve the value of the connection 
            //   string via the SqlServer property
            string cxnConfiguration = Configuration.GetValueOrThrow<string>("DbContexts:ConfigurationDbContext:ConnectionString",null,true);
            string cxnPersistedGrant = Configuration.GetValueOrThrow<string>("DbContexts:PersistedGrantDbContext:ConnectionString", null, true);
            string cxnAspNetIdentity = Configuration.GetValueOrThrow<string>("DbContexts:DomainIdentityDbContext:ConnectionString", null, true);


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


            services.Configure<CentralAdminOptions>(Configuration.GetSection("CentralAdmin"));
            services.Configure<ClaimsPrincipalFactoryOptions>(Configuration.GetSection("ClaimsPrincipalFactory"));
            services.AddSingleton<CentralAdmin>();


            services.AddDbContext<DomainIdentityDbContext>(options =>
                    options.UseSqlServer(cxnAspNetIdentity,
                        sql => sql.MigrationsAssembly(migrationsAssembly)));

            services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddUserStore<DomainUserStore>()
                .AddUserManager<UserManager<DomainUser>>()
                .AddClaimsPrincipalFactory<DomainUserClaimsPrincipalFactory>();


            services.AddTransient<IEmailSender, MockEmailSender>();

            //replace Identity Server's ProfileService with a profile service that determines
            //which claims to retrieve for a user/client as configured in the database
            services.Replace(ServiceDescriptor.Transient<IProfileService, DomainIdentityProfileService>());

            services.AddOidcLogging(Configuration);


            //services.AddAuthorization(options =>
            //{
            //    var settings = new AdministrationSettings();
            //    Configuration.GetSection("Administration").Bind(settings);
            //    if(settings.PolicyType == PolicyType.Unconfigured && settings.PolicyName == default)
            //        throw new System.Exception("IdentityServer configuration does not contain an Administration section that can bind to an AdministrationSettings object.");

            //    options.AddPolicy(settings.PolicyName, policy =>
            //    {
            //        if (settings.PolicyType == PolicyType.Open)
            //            policy.RequireAssertion(context => true); //open
            //        else if (settings.PolicyType == PolicyType.Role)
            //            policy.RequireRole(settings.RoleName);
            //    });
            //});

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
