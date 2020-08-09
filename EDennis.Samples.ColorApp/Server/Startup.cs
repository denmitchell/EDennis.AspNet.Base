using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using EDennis.Samples.ColorApp.Server.Data;
using EDennis.Samples.ColorApp.Server.Models;
using Microsoft.OpenApi.Models;
using EDennis.AspNetIdentityServer;
using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;

namespace EDennis.Samples.ColorApp.Server {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<DomainIdentityDbContext>();

            //add reference to EDennis.AspNetIdentityServer

            services.AddIdentityServer(config => { })
                //.AddAspNetIdentity<DomainUser>() //may need to replace default UserClaimsPrincipalFactory with mine
                //.AddApiResources() //from Configuration["IdentityServer:Resources"]
                //.AddClients() //from Configuration["IdentityServer:Clients"]
                //.AddIdentityResources() //from Configuration["IdentityServer:IdentityResources"]
                .AddConfigurationStore(options => {new DefaultConfigurationStoreOptions().Load(options); }) //Need to configure the db context and set table names here just like DefaultConfigurationStoreOptions
                .AddOperationalStore(options => { new DefaultOperationalStoreOptions().Load(options); })//Need to configure the db context and set table names here just like DefaultOperationalStoreOptions
                .AddProfileService<DomainIdentityProfileService>()
                .AddApiAuthorization<DomainUser, PersistedGrantDbContext>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ColorProxyApi", Version = "v1" });
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

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Color Proxy API V1");
            });

        }
    }
}
