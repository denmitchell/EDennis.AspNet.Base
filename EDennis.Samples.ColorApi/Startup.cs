using EDennis.NetStandard.Base;
using EDennis.Samples.ColorApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace EDennis.Samples.ColorApi {
    public class Startup {
        public Startup(IConfiguration configuration, IHostEnvironment env) {
            Configuration = configuration;
            HostEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment {get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddCors();
            services.AddControllers(options=>options.AddDefaultPolicies<Startup>(services,HostEnvironment,Configuration));
            services.AddAuthorization(options => {
                IServiceCollectionExtensions_DefaultPolicies.LoadDefaultPolicies<Startup>(options, new List<string> { "scope" });
                });

            var cxnString = Configuration.GetValueOrThrow<string>("ConnectionStrings:ColorContext",null,true);
            services.AddScoped<DbContextProvider<ColorContext>>();
            services.AddDbContext<ColorContext>(options => {
                options.UseSqlServer(cxnString,opt=>opt.MigrationsAssembly(typeof(ColorContext).Assembly.GetName().Name));
                options.EnableSensitiveDataLogging();
            });

            services.AddSingleton(new TransactionCache<ColorContext>());

            //with AddSecureTokenService, I may only need
            //services.AddAuthentication();

            //TODO: SEE WHETHER .AddJwtBearer is needed.

            services.AddAuthentication("Bearer")
                       .AddJwtBearer("Bearer", options =>
                       {
                           options.Authority = "https://localhost:5000";

                           options.TokenValidationParameters = new TokenValidationParameters {
                               ValidateAudience = false
                           };
                       });
            

            services.AddMockClaimsPrincipal(Configuration);
            services.AddHeaderToClaims(Configuration);
            services.AddCachedTransaction(Configuration);
            //services.AddHttpLogging(Configuration);
            //for resolving parent claims to app-level child claims defined in configuration
            services.AddChildClaimCache(Configuration);
            services.AddScopedRequestMessage(Configuration);


            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ColorApi", Version = "v1" });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {


            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseMockClaimsPrincipalFor("/api/Rgb");

            // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseAuthentication();
            app.UseHeaderToClaimsFor("/api/Rgb");
            app.UseAuthorization();

            app.UseCachedTransactionFor<ColorContext>("/api/Rgb");
            //app.UseHttpLoggingFor("/api/Rgb");
            app.UseScopedRequestMessageFor("/api/Rgb");

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Color API V1");
            });


        }
    }
}
