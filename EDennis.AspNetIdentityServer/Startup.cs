using EDennis.NetApp.Base;
using EDennis.NetStandard.Base;
using IdentityServer4.AspNetIdentity;
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
            string cxnAspNetIdentity = Configuration.GetValueOrThrow<string>("ConnectionStrings:DomainIdentityDbContext", null, true);


            services.AddScoped<IUserClaimsPrincipalFactory<DomainUser>, DomainUserClaimsPrincipalFactory>();

            services.AddIdentityServer()
                .AddConfigurationStore(options => {
                    new DefaultConfigurationStoreOptions().Load(options);
                    options.ConfigureDbContext = b => b.UseSqlServer(cxnAspNetIdentity,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options => {
                    new DefaultOperationalStoreOptions().Load(options);
                    options.ConfigureDbContext = b => b.UseSqlServer(cxnAspNetIdentity,
                        sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddDeveloperSigningCredential()
                .AddAspNetIdentity<DomainUser>()
                .AddProfileService<ProfileService<DomainUser>>();


            services.Configure<ClaimsPrincipalFactoryOptions>(Configuration.GetSection("ClaimsPrincipalFactory"));
            services.Configure<CentralAdminOptions>(Configuration.GetSection("CentralAdmin"));
            services.AddSingleton<CentralAdmin>();


            services.AddDbContext<DomainIdentityDbContext>(options =>
                    options.UseSqlServer(cxnAspNetIdentity,
                        sql => sql.MigrationsAssembly(migrationsAssembly)));

            services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddUserStore<DomainUserStore>()
                .AddUserManager<UserManager<DomainUser>>()
                .AddClaimsPrincipalFactory<DomainUserClaimsPrincipalFactory>();

            services.AddCors(options =>
            {
                options.AddPolicy("Cors",
                    builder => builder.WithOrigins("https://localhost:44305"));
            });


            services.AddTransient<IEmailSender, MockEmailSender>();

            services.AddOidcLogging(Configuration);

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
