using EDennis.AspNetIdentityServer;
using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace EDennis.HostedBlazor.Base {
    public static class IServiceCollectionExtensions_Blazor {

        /// <summary>
        /// Configures ASP.NET Identity and built-in Identity Server for Blazor hosted applications.
        /// see https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-3.1&tabs=visual-studio
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static IServiceCollection AddIntegratedIdentityServerAndAspNetIdentity(
            this IServiceCollection services, IConfiguration config,
            string configKey = "ConnectionStrings:DomainIdentityDbContext",
            Action<IdentityOptions> configureOptions = null) {


            //Step 1: Add the DbContext for ASP.NET Identity
            var cxnString = config.GetValueOrThrow<string>(configKey);
            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(cxnString));


            //Step 2: Add common ASP.NET Identity services, including default UI
            //replacing call to services.AddDefaultIdentity<DomainUser>(options => { options.SignIn.RequireConfirmedAccount = true; });
            services.AddDefaultIdentity<DomainUser>(options => { options.SignIn.RequireConfirmedAccount = true; })
                .AddUserStore<DomainUserStore>()
                .AddClaimsPrincipalFactory<DomainUserClaimsPrincipalFactory>();
            ;

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

        }

        public static void ReplaceServiceImplementations<IServiceType, TReplacementImplementation>(this IServiceCollection services,
            ServiceLifetime serviceLifeTime)
            where IServiceType : class
            where TReplacementImplementation : class, IServiceType {

            //IUserClaimsPrincipalFactory<DomainUser>, DomainUserClaimsPrincipalFactory
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
