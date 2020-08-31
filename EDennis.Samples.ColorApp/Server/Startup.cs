using EDennis.HostedBlazor.Base;
using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

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
            services.AddIntegratedIdentityServerAndAspNetIdentity(Configuration, 
               "ConnectionStrings:DomainIdentityDbContext");


            services.AddControllersWithViews(options=>
                //add default policies that allow pattern matching on scopes
                options.AddDefaultPolicies<Startup>(services, HostEnvironment, Configuration));

            services.AddAuthorization(options => {
                IServiceCollectionExtensions_DefaultPolicies.LoadDefaultPolicies<Startup>(options, new List<string> { "scope" });
            });


            services.AddRazorPages();

            services.AddHttpLogging(Configuration);


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
