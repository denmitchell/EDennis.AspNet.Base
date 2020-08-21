using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EDennis.AspNetIdentityServer {
    public static class IServiceCollectionExtensions {

        /// <summary>
        /// Configures ASP.NET Identity and built-in Identity Server for Blazor hosted applications.
        /// see https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-3.1&tabs=visual-studio
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static IServiceCollection AddIntegratedIdentityServerAndAspNetIdentity(this IServiceCollection services, IConfiguration config,
            string configKey = "ConnectionStrings:DomainIdentityDbContext") {

            //Step 1: Add the DbContext for ASP.NET Identity
            var cxnString = config.GetValueOrThrow<string>(configKey);
            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(cxnString));


            //Step 2: Add built-in Identity Server, but point to central Identity Server stores
            services.AddIdentityServer(config => { })
                //Add integration between Identity Server and ASP.NET Identity
                .AddAspNetIdentity<DomainUser>()
                .AddApiAuthorization<DomainUser, PersistedGrantDbContext>()
                //for in-memory objects from configuration ...
                //.AddApiResources() //from Configuration["IdentityServer:Resources"]
                //.AddClients() //from Configuration["IdentityServer:Clients"]
                //.AddIdentityResources() //from Configuration["IdentityServer:IdentityResources"]
                //Ensure that the current, default models are used for Identity Server stores
                .AddConfigurationStore(options => { new DefaultConfigurationStoreOptions().Load(options); }) //Need to configure the db context and set table names here just like DefaultConfigurationStoreOptions
                .AddOperationalStore(options => { new DefaultOperationalStoreOptions().Load(options); })//Need to configure the db context and set table names here just like DefaultOperationalStoreOptions
                                                                                                        //Add the custom profile service, which uses the UserClientApplicationRole view
                .AddProfileService<DomainIdentityProfileService>();

            //Step 3: Add common ASP.NET Identity services, including default UI
            services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                //Point to centralized database
                .AddEntityFrameworkStores<DomainIdentityDbContext>()
                //Use custom ClaimsPrincipalFactory that uses DomainRole (including Application Name)
                .AddClaimsPrincipalFactory<DomainUserClaimsPrincipalFactory>()
                //Use custom role validator to prevent failing on duplicate role name;
                //  with DomainRole, different applications can have the same role name
                .AddRoleValidator<DomainRoleValidator>();

            //TODO: see if this is needed
            services.Replace(new ServiceDescriptor(typeof(IUserClaimsPrincipalFactory<DomainUser>),
                typeof(DomainUserClaimsPrincipalFactory), ServiceLifetime.Scoped));


            services.AddAuthentication()
                .AddIdentityServerJwt();

            return services;

        }

    }
}
