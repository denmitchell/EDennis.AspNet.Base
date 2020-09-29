using EDennis.NetApp.Base;
using EDennis.NetStandard.Base;
using IdentityModel.AspNetCore;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ProxyKit;
using System.IdentityModel.Tokens.Jwt;

namespace EDennis.Samples.ColorApp.Server {
    public class Startup {
        public Startup(IConfiguration configuration, IHostEnvironment env) {
            Configuration = configuration;
            HostEnvironment = env;
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {


            services.AddProxy();
            services.AddAccessTokenManagement();

            services.AddControllers();
            services.AddDistributedMemoryCache();


            //Proxy forwarding with OIDC user access token 
            //(Adapted from Dominick Baier: https://github.com/leastprivilege/AspNetCoreSecuritySamples/tree/aspnetcore21/BFF)
            services.AddOpenIdConnectBFF(Configuration);


            services.AddRazorPages();


            //for mocking the user/client
            services.AddMockClaimsPrincipal(Configuration);

            //for creating a cookie that holds the database transaction key
            services.AddCachedTransactionCookie(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
                app.UseCachedTransactionCookieFor("Rgb"); //to auto-rollback database
            } else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();

            //Proxy forwarding with OIDC user access token 
            //(Adapted from Dominick Baier: https://github.com/leastprivilege/AspNetCoreSecuritySamples/tree/aspnetcore21/BFF)
            //
            //NOTE: for routes that match "Apis:{SomeApiKey}" = {SomeApiControllerUrl}
            //      in Configuration, the request will be forwarded to that API, 
            //      and the rest of the pipeline is short-circuited.
            app.UseSecureForwarding(Configuration, "Apis");


            //-----------------------------------------------------------------
            //Continue the pipeline for routes that don't match any key in the 
            //      Apis section of Configuration ...
            //-----------------------------------------------------------------

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers()
                    .RequireAuthorization();
                endpoints.MapFallbackToFile("index.html");
            });

        }
    }
}
