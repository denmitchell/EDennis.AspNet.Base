using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// A UserClaimsPrincipalFactory that limits role claims to those relevant to the current
    /// application.  
    /// NOTE: This factory is used with the "Domain Identity" classes (DomainUserManager, 
    /// DomainRoleManager, DomainUser, DomainRole, IdentityApplication, and IdentityOrganization),
    /// which supports centralized management of user security across different applications and
    /// organizations. This is not a multi-tenant architecture, but an architecture that allows
    /// user management across applications and allows user management to be delegated 
    /// to organization admins.
    /// </summary>
    /// <typeparam name="TUser">DomainUser or subclass</typeparam>
    /// <typeparam name="TRole">DomainRole or sublcass</typeparam>
    public class DomainUserClaimsPrincipalFactory<TUser, TRole, TContext> : UserClaimsPrincipalFactory<TUser, TRole>
        where TUser : DomainUser, new()
        where TRole : DomainRole
        where TContext : DomainIdentityDbContext<TUser,TRole> {

        private readonly string _applicationName;
        private readonly DomainUserManager<TUser,TRole,TContext> _domainUserManager;


        public DomainUserClaimsPrincipalFactory(DomainUserManager<TUser,TRole,TContext> userManager, 
            DomainRoleManager<TUser,TRole,TContext> roleManager,
            IOptions<IdentityOptions> options, IHostEnvironment env) : base(userManager, roleManager, options) {
            _applicationName = env.ApplicationName;
            _domainUserManager = userManager;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user) {

            //get new ClaimsIdentity with non-role claims first
            var identity = await GenerateNonRoleClaimsAsync(user);

            //add role claims, but limit to current application
            foreach (var role in await _domainUserManager.GetRolesAsync(user, _applicationName)) {
                //if Name property = Admin@CT.DDS.PRAT, then claim = "role", "Admin"
                identity.AddClaim(new Claim(Options.ClaimsIdentity.RoleClaimType, role.Title));
                identity.AddClaims(await RoleManager.GetClaimsAsync(role));
            }
            
            return identity;
        }


        /// <summary>
        /// from UserClaimsPrincipalFactory<TUser>.GenerateClaimsAsync(TUser user)
        /// see https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/UserClaimsPrincipalFactory.cs
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected virtual async Task<ClaimsIdentity> GenerateNonRoleClaimsAsync(TUser user) {
            var userId = await UserManager.GetUserIdAsync(user);
            var userName = await UserManager.GetUserNameAsync(user);
            var id = new ClaimsIdentity("Identity.Application", // REVIEW: Used to match Application scheme
                Options.ClaimsIdentity.UserNameClaimType,
                Options.ClaimsIdentity.RoleClaimType);
            id.AddClaim(new Claim(Options.ClaimsIdentity.UserIdClaimType, userId));
            id.AddClaim(new Claim(Options.ClaimsIdentity.UserNameClaimType, userName));
            if (UserManager.SupportsUserEmail) {
                var email = await UserManager.GetEmailAsync(user);
                if (!string.IsNullOrEmpty(email)) {
                    id.AddClaim(new Claim(ClaimTypes.Email, email));
                }
            }
            if (UserManager.SupportsUserSecurityStamp) {
                id.AddClaim(new Claim(Options.ClaimsIdentity.SecurityStampClaimType,
                    await UserManager.GetSecurityStampAsync(user)));
            }
            if (UserManager.SupportsUserClaim) {
                id.AddClaims(await UserManager.GetClaimsAsync(user));
            }
            return id;
        }

    }
}
