using EDennis.AspNet.Base;
using Hr.PersonApi.Models;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Linq;

namespace Hr.PersonApi {
    public class Startup {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment HostingEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers(options=> {
/*
                options.Conventions.Add(new DefaultAuthorizationPolicyConvention(
                    HostingEnvironment.ApplicationName, Configuration));
*/
            });
/*
            var oidcOptions = new OidcOptions();
            Configuration.GetSection("Security:OidcOptions").Bind(oidcOptions);

            //add an AuthorizationPolicyProvider which generates default
            //policies upon first access to any controller action
            
            services.AddSingleton<IAuthorizationPolicyProvider>(factory => {
                var logger = factory.GetRequiredService<ILogger<DefaultPoliciesAuthorizationPolicyProvider>>();
                return new DefaultPoliciesAuthorizationPolicyProvider(
                        Configuration, oidcOptions, logger);

                });
*/
            services.AddOData();

            var cxnString = Configuration["ConnectionStrings:HrContext"];

            services.AddDbContext<HrContext>(options => {
                options.UseSqlServer(cxnString);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Person API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Person API V1");
            });

            app.UseRouting();

/*
            app.UseAuthorization();
*/
            app.UseEndpoints(endpoints => {
                endpoints.Select().Filter(); //odata support
                endpoints.MapControllers();
            });
        }
    }
}
