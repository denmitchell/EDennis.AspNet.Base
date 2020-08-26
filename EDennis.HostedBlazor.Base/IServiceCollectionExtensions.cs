using EDennis.AspNetIdentityServer;
using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static IServiceCollection AddIntegratedIdentityServerAndAspNetIdentity<TAppClaimEncoder>(
            this IServiceCollection services, IConfiguration config,
            string configKey = "ConnectionStrings:DomainIdentityDbContext",
            Action<IdentityOptions> configureOptions = null)

            where TAppClaimEncoder : class, IAppClaimEncoder {


            //try add IAppClaimEncoder implementation
            services.TryAddSingleton<IAppClaimEncoder, TAppClaimEncoder>();


            //Step 1: Add the DbContext for ASP.NET Identity
            var cxnString = config.GetValueOrThrow<string>(configKey);
            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(cxnString));


            //Step 2: Add common ASP.NET Identity services, including default UI
            //replacing call to services.AddDefaultIdentity<DomainUser>(options => { options.SignIn.RequireConfirmedAccount = true; });
            //services.AddDefaultIdentity<DomainUser>(options => { options.SignIn.RequireConfirmedAccount = true; });


            //https://github.com/dotnet/aspnetcore/blob/bfec2c14be1e65f7dd361a43950d4c848ad0cd35/src/Identity/UI/src/IdentityServiceCollectionUIExtensions.cs
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityServerJwt()
            .AddIdentityCookies(o => { });


            //https://github.com/dotnet/aspnetcore/blob/555b506a97a188583df2872913cc40b59e563da6/src/Identity/Extensions.Core/src/IdentityServiceCollectionExtensions.cs
            // Services identity depends on
            services.AddOptions().AddLogging();

            // Services used by identity
            //services.TryAddScoped<IUserValidator<DomainUser>, UserValidator<DomainUser>>(); //handled with IdentityBuilder below
            //services.TryAddScoped<IPasswordValidator<DomainUser>, PasswordValidator<DomainUser>>(); //handled with IdentityBuilder below
            services.TryAddScoped<IPasswordHasher<DomainUser>, PasswordHasher<DomainUser>>();
            services.TryAddScoped<ILookupNormalizer, NoEffectNormalizer>(); // don't use UpperInvariantLookupNormalizer> because managers use case-insensitive EF.Functions.Like
            services.TryAddScoped<IUserConfirmation<DomainUser>, DefaultUserConfirmation<DomainUser>>();
            //services.TryAddScoped<IdentityErrorDescriber>(); //handled with IdentityBuilder below
            //services.TryAddScoped<IUserClaimsPrincipalFactory<DomainUser>, DomainUserClaimsPrincipalFactory>(); //handled with replace service below
            //services.TryAddScoped<UserManager<DomainUser>>(); //handled with IdentityBuilder below

            configureOptions ??= options =>
            {
                options.Stores.MaxLengthForKeys = 128;
                options.SignIn.RequireConfirmedAccount = true;
            };

            services.Configure(configureOptions);




            //explicitly instantiating IdentityBuilder in order to pass in
            //type of TUser and type of TRole
            var ibuilder = new IdentityBuilder(typeof(DomainUser), typeof(DomainRole), services)
                .AddUserStore<DomainUserStore>()
                .AddRoleStore<DomainRoleStore>()
                .AddUserManager<DomainUserManager>()
                .AddRoleManager<DomainRoleManager>()
                .AddUserValidator<UserValidator<DomainUser>>() //https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/UserValidator.cs
                .AddRoleValidator<DomainRoleValidator>()
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



            var isBuilder = services.AddIdentityServer(config => { })
                //.AddApiAuthorization<DomainUser, PersistedGrantDbContext>()
                .AddAspNetIdentity<DomainUser>()
                .AddSigningCredentials()
                .AddConfigurationStore<ConfigurationDbContext>()
                .AddOperationalStore<PersistedGrantDbContext>()
                //Add the custom profile service, which uses the UserClientApplicationRole view
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
