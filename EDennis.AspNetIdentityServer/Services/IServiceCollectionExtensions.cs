using EDennis.NetStandard.Base;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;

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
        public static IServiceCollection AddIntegratedIdentityServerAndAspNetIdentity<TAppClaimEncoder>(this IServiceCollection services, IConfiguration config,
            string configKey = "ConnectionStrings:DomainIdentityDbContext")
            where TAppClaimEncoder : class, IAppClaimEncoder {


            //try add IAppClaimEncoder implementation
            services.TryAddSingleton<IAppClaimEncoder, TAppClaimEncoder>();


            //Step 1: Add the DbContext for ASP.NET Identity
            var cxnString = config.GetValueOrThrow<string>(configKey);
            services.AddDbContext<DomainIdentityDbContext>(options =>
                options.UseSqlServer(cxnString));


            //TODO: Check whether Validators are working OK

            //Step 2: Add common ASP.NET Identity services, including default UI
            services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddUserStore<DomainUserStore>()
                .AddRoleStore<DomainRoleStore>()
                .AddUserManager<DomainUserManager>()
                .AddRoleManager<DomainRoleManager>()
                .AddUserValidator<DomainUser>()
                .AddRoleValidator<DomainRole>();


            //Step 3: Add built-in Identity Server, but point to central Identity Server stores
            var isBuilder = services.AddIdentityServer(config => { });

            //note: workaround to prevent double-registering services
            var cpfServices = services.Where(s => s.ServiceType == typeof(IUserClaimsPrincipalFactory<DomainUser>)).ToArray();
            for (int i = 0; i < cpfServices.Length; i++) {
                var cpfService = cpfServices[i];
                services.Remove(cpfService);
            }

            isBuilder.Services.AddScoped<IUserClaimsPrincipalFactory<DomainUser>, DomainUserClaimsPrincipalFactory>();

            //Add integration between Identity Server and ASP.NET Identity
            isBuilder.AddAspNetIdentity<DomainUser>()
                .AddSigningCredentials();

                //for in-memory objects from configuration ...
                //.AddApiResources() //from Configuration["IdentityServer:Resources"]
                //.AddClients() //from Configuration["IdentityServer:Clients"]
                //.AddIdentityResources() //from Configuration["IdentityServer:IdentityResources"]
            isBuilder
                .AddConfigurationStore<ConfigurationDbContext>(options => { 
                    new DefaultConfigurationStoreOptions().Load(options); 
                })
                .AddOperationalStore<PersistedGrantDbContext>(options => { 
                    new DefaultOperationalStoreOptions().Load(options); 
                })
                                                                                                        
                //Add the custom profile service, which uses the UserClientApplicationRole view
                .AddProfileService<DomainIdentityProfileService>();

            services.AddAuthentication()
                .AddIdentityServerJwt();

            return services;

        }

    }
}
