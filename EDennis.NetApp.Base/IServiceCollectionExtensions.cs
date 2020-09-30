using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Linq;

namespace EDennis.NetApp.Base {

    public static class IServiceCollectionExtensions_NetCoreApp {



        public static IServiceCollection AddOpenIdConnectBFF(this IServiceCollection services,
            IConfiguration config, 
            string openIdConnectConfigKey = "Security:OpenIdConnect") {


            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("cookies", options =>
            {
                options.Cookie.Name = "bff";
                options.Cookie.SameSite = SameSiteMode.Strict;
            })
            .AddOpenIdConnect("oidc", options =>
            {
                var settings = new OpenIdConnectSettings();
                config.BindSectionOrThrow(openIdConnectConfigKey, settings);

                options.Authority = settings.Authority;
                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;

                options.ResponseType = "code";
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SaveTokens = true;

                options.Scope.Clear();
                foreach (var scope in settings.Scope)
                    options.Scope.Add(scope);
                if (!options.Scope.Contains("openid"))
                    options.Scope.Add("openid");
                if (!options.Scope.Contains("offline_access"))
                    options.Scope.Add("offline_access");

                options.TokenValidationParameters = new TokenValidationParameters {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });

            return services;

        }





        /// <summary>
        /// Configures authentication to use the AspNetIdentityServer for User/Claims store
        /// and for the JwtBearer provider
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="identityServerAuthorityConfigKey"></param>
        /// <param name="roleClaimType"></param>
        /// <param name="nameClaimType"></param>
        /// <param name="dbContextConfigKey"></param>
        /// <returns></returns>
        public static IServiceCollection AddAspNetIdentityServer(
                    this IServiceCollection services, IConfiguration config,
                    string identityServerAuthorityConfigKey,
                    string roleClaimType,
                    string nameClaimType="name",
                    string dbContextConfigKey = "ConnectionStrings:DomainIdentityDbContext") {



            //Step 1: Add the DbContext for ASP.NET Identity
            var cxnString = config.GetValueOrThrow<string>(dbContextConfigKey);
            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(cxnString));


            //Step 2: Add common ASP.NET Identity services, including default UI
            services.AddDefaultIdentity<DomainUser>(options => {
                options.SignIn.RequireConfirmedAccount = true;
            })
                //DomainUserStore has special handling for the LockoutBegin property; 
                //So, instead of AddEntityFrameworkStores<ApplicationDbContext> ...
                .AddUserStore<DomainUserStore>();


            //Step 3: Use IdentityServer as the JwtBearer provider
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                           .AddJwtBearer(options => {
                               options.Authority = config[identityServerAuthorityConfigKey];
                               options.Audience = roleClaimType;
                               options.TokenValidationParameters = new TokenValidationParameters() {
                                   RoleClaimType = roleClaimType,
                                   NameClaimType = nameClaimType
                               };
                           });


            return services;

        }

        public static void ReplaceServiceImplementations<IServiceType, TReplacementImplementation>(this IServiceCollection services,
            ServiceLifetime serviceLifeTime)
            where IServiceType : class
            where TReplacementImplementation : class, IServiceType {

            //note: workaround to prevent double-registering services
            var servicesToReplace = services.Where(s => s.ServiceType == typeof(IServiceType)).ToArray();
            for (int i = 0; i < servicesToReplace.Length; i++) {
                var serviceToReplace = servicesToReplace[i];
                if (serviceToReplace.ImplementationType != typeof(TReplacementImplementation)
                    || serviceToReplace.Lifetime != serviceLifeTime)
                    services.Remove(serviceToReplace);
            }

            //Register service, if not already registered
            if (!services.Any(s =>
                s.ServiceType == typeof(IServiceType)
                    && s.ImplementationType == typeof(TReplacementImplementation)
                    && s.Lifetime == serviceLifeTime)) {
                switch (serviceLifeTime) {
                    case ServiceLifetime.Singleton:
                        services.AddSingleton<IServiceType, TReplacementImplementation>();
                        break;
                    case ServiceLifetime.Scoped:
                        services.AddScoped<IServiceType, TReplacementImplementation>();
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient<IServiceType, TReplacementImplementation>();
                        break;
                }
            }

        }

        public static void RemoveServiceImplementations<IServiceType>(this IServiceCollection services)
            where IServiceType : class {

            //note: workaround to prevent double-registering services
            var servicesToRemove = services.Where(s => s.ServiceType == typeof(IServiceType)).ToArray();
            for (int i = 0; i < servicesToRemove.Length; i++) {
                services.Remove(servicesToRemove[i]);
            }

        }


        public static void RemoveServiceImplementations(this IServiceCollection services, string serviceTypeFullNameStartsWith) {

            //note: workaround to prevent double-registering services
            var servicesToRemove = services.Where(s => s.ServiceType.FullName.StartsWith(serviceTypeFullNameStartsWith)).ToArray();
            for (int i = 0; i < servicesToRemove.Length; i++) {
                Debug.WriteLine(servicesToRemove[i].ServiceType.Name);
                services.Remove(servicesToRemove[i]);
            }

        }


    }

}
