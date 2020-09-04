using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Services;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

namespace EDennis.NetApp.Base {

    public static class IServiceCollectionExtensions_NetCoreApp {


        /// <summary>
        /// Configures ASP.NET Identity and built-in Identity Server for Blazor hosted applications.
        /// see https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-3.1&tabs=visual-studio
        /// 
        /// 
        /// Sample Configuration Section:
        /// 
        /// "IdentityServer": {
        ///    "Key": {
        ///      "Type": "Development"
        ///    }
        ///    "Clients": {
        ///      "EDennis.Samples.ColorApp.Client": {
        ///        "Profile": "IdentityServerSPA"
        ///      }
        ///    "ApiResources": [
        ///        {
        ///            "Name": "EDennis.Samples.ColorApp.Server",
        ///            "UserClaims": [
        ///                "sub",
        ///                "name",
        ///                "email",
        ///                "phone_number",
        ///                "organization",
        ///                "organization_admin_for",
        ///                "super_admin",
        ///                "role:EDennis.Samples.ColorApp.Server",
        ///                "locked"
        ///            ],
        ///            "Scopes": [
        ///                "EDennis.Samples.ColorApp.Server",
        ///            ]
        ///    }
        ///    ]
        ///}
        ///  
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="dbContextConfigKey"></param>
        /// <returns></returns>
        public static IServiceCollection AddIntegratedIdentityServerAndAspNetIdentity(
                    this IServiceCollection services, IConfiguration config,
                    string dbContextConfigKey = "ConnectionStrings:DomainIdentityDbContext",
                    string identityServerConfigKey = "Security:IdentityServer") {


            //Step 1. Get options from configuration
            var apiAuthorizationOptions = new ApiAuthorizationOptions();
            config.BindSectionOrThrow(identityServerConfigKey, apiAuthorizationOptions);
            services.Configure<ApiAuthorizationOptions>(config.GetSection(identityServerConfigKey));


            //Step 2: Add the DbContext for ASP.NET Identity
            var cxnString = config.GetValueOrThrow<string>(dbContextConfigKey);
            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(cxnString));


            //Step 3: Add common ASP.NET Identity services, including default UI
            services.AddDefaultIdentity<DomainUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })

                //DomainUserStore has special handling for the LockoutBegin property; 
                //So, instead of AddEntityFrameworkStores<ApplicationDbContext> ...
                .AddUserStore<DomainUserStore>()

                //DomainUserClaimsPrincipalFactory adds user properties as claims
                //  by calling DomainUser.ToClaims();
                //So, instead of default UserClaimsPrincipalFacory ...
                .AddClaimsPrincipalFactory<DomainUserClaimsPrincipalFactory>();
            ;


            //Step 4: Add integrated identity server, with clients and API resources
            //        created from configuration
            services.AddIdentityServer()
                .AddApiAuthorization<DomainUser, PersistedGrantDbContext>(opt =>
                {
                    //having invoked Configure on the options, this may not be needed ...
                    opt.Clients = apiAuthorizationOptions.Clients;
                    opt.ApiResources = apiAuthorizationOptions.ApiResources;
                })
                //provides the integration with ASP.NET identity;
                //NOTE: with this call, IdentityServer should use DomainManager<DomainUser>, 
                //      which should in turn use DomainUserStore
                .AddAspNetIdentity<DomainUser>();

            
            //Step 5: Ensure correct implementations for IUserClaimsPrincipalFactory and IProfileService,
            //        As these may have been altered by the call to .AddAspNetIdentity, above.
            services.ReplaceServiceImplementations<IUserClaimsPrincipalFactory<DomainUser>, DomainUserClaimsPrincipalFactory>(ServiceLifetime.Scoped);
            services.Replace(ServiceDescriptor.Transient<IProfileService, DomainIdentityProfileService>());


            return services;

            #region old code that replaced .AddApiAuthorization
            /*

            //explicitly instantiating IdentityBuilder in order to pass in
            //type of TUser
            var ibuilder = new IdentityBuilder(typeof(DomainUser), services)
                .AddUserStore<DomainUserStore>()
                .AddUserManager<UserManager<DomainUser>>()
                .AddUserValidator<UserValidator<DomainUser>>() //https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/UserValidator.cs
                .AddPasswordValidator<PasswordValidator<DomainUser>>()
                .AddErrorDescriber<IdentityErrorDescriber>()
                .AddDefaultTokenProviders()
                .AddDefaultUI()
                ;



            //Step 3: Add built-in Identity Server with ASP.NET Identity integration,
            //but point to central Identity Server stores
            //https://github.com/IdentityServer/IdentityServer4/blob/18897890ce2cb020a71b836db030f3ed1ae57882/src/IdentityServer4/src/Configuration/DependencyInjection/IdentityServerServiceCollectionExtensions.cs
            //https://github.com/IdentityServer/IdentityServer4/blob/18897890ce2cb020a71b836db030f3ed1ae57882/src/AspNetIdentity/src/IdentityServerBuilderExtensions.cs

            services.ReplaceServiceImplementations<IUserClaimsPrincipalFactory<DomainUser>, DomainUserClaimsPrincipalFactory>(ServiceLifetime.Scoped);


            services.AddSingleton(new ConfigurationStoreOptions());
            services.AddSingleton(new OperationalStoreOptions());
            services.AddDbContext<IConfigurationDbContext, ConfigurationDbContext>(options =>
                options.UseSqlServer(cxnString));
            services.AddDbContext<IPersistedGrantDbContext, PersistedGrantDbContext>(options =>
                options.UseSqlServer(cxnString));


            var isBuilder = services.AddIdentityServer()
                .AddApiAuthorization<DomainUser, PersistedGrantDbContext>()
                .AddResourceStore<ResourceStore>()
                .AddClientStore<ClientStore>()
                .AddPersistedGrantStore<PersistedGrantStore>()
                .AddProfileService<DomainIdentityProfileService>();



            //isBuilder.Services.Configure((ApiAuthorizationOptions o)=> { });


            //ensure that custom DomainUserClaimsPrincipalFactory, rather than UserClaimsFactory, is used
            services.ReplaceServiceImplementations<IUserClaimsPrincipalFactory<DomainUser>, DomainUserClaimsPrincipalFactory>(ServiceLifetime.Scoped);

            return services;
            */

            #endregion
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


    }

}
