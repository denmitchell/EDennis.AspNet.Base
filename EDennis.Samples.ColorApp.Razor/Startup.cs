using EDennis.NetStandard.Base;
using EDennis.NetApp.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace EDennis.Samples.ColorApp.Razor {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddRazorPages();

            //for generating the OAuth Access Token
            services.AddSecureTokenService<MockTokenService>(Configuration);

            services.AddAuthentication()
                .AddOpenIdConnect(Configuration);

            //for propagating headers and cookies to child API (ColorApi)
            services.AddScopedRequestMessage(Configuration);

            //for communicating with the child API (ColorApi)
            services.AddApiClient<RgbApiClient>(Configuration);

            //for mocking the user/client
            services.AddMockClaimsPrincipal(Configuration);

            //for resolving parent claims to app-level child claims defined in configuration
            services.AddChildClaimCache(Configuration);

            //for propagating user claims to the child API (ColorApi) via headers
            services.AddClaimsToHeader(Configuration);

            //for creating a cookie that holds the database transaction key
            services.AddCachedTransactionCookie(Configuration);

            //services.AddServerSideBlazor(); //for .razor components

            //for interactively testing the APIs directly
            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ColorApi (via Razor App)", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseMockClaimsPrincipalFor("/Rgb");
            app.UseAuthentication();
            app.UseClaimsToHeaderFor("/Rgb");
            app.UseAuthorization();
            app.UseCachedTransactionCookieFor("/Rgb");
            app.UseScopedRequestMessageFor("/Rgb");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                //endpoints.MapBlazorHub(); //for .razor components
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ColorApi Via Blazor App");
            });

        }
    }
}
