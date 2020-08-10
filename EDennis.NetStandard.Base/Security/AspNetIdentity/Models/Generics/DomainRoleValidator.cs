using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Ensures that RoleManager requires uniqueness of combination of 
    ///   ApplicationId and NormalizedRoleName, rather than just NormalizedRoleName
    ///   
    /// NOTE: In Startup.ConfigureServices:
    ///         services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
    ///            .AddEntityFrameworkStores<DomainIdentityDbContext>()
    ///            .AddRoleValidator<DomainRoleValidator<Type Arguments>>(); //add this line
    ///   
    /// Adapted from https://stackoverflow.com/a/59466933
    /// </summary>
    /// <typeparam name="TUser">User Type</typeparam>
    /// <typeparam name="TOrganization">Organization Type</typeparam>
    /// <typeparam name="TUserClaim">User Claim Type</typeparam>
    /// <typeparam name="TUserLogin">User Login Type</typeparam>
    /// <typeparam name="TUserToken">User Token Type</typeparam>
    /// <typeparam name="TRole">Role Type</typeparam>
    /// <typeparam name="TApplication">Application Type</typeparam>
    /// <typeparam name="TRoleClaim">Role Claim Type</typeparam>
    /// <typeparam name="TUserRole">User Role Type</typeparam>
    public class DomainRoleValidator<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole> : RoleValidator<TRole>
        where TUser : DomainUser<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TOrganization : DomainOrganization<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserClaim : DomainUserClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserLogin : DomainUserLogin<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserToken : DomainUserToken<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRole : DomainRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TApplication : DomainApplication<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRoleClaim : DomainRoleClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserRole : DomainUserRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new() {
        public override async Task<IdentityResult> ValidateAsync(RoleManager<TRole> manager, TRole role) {
            var roleName = await manager.GetRoleNameAsync(role);
            if (string.IsNullOrWhiteSpace(roleName)) {
                return IdentityResult.Failed(new IdentityError {
                    Code = "RoleNameIsNotValid",
                    Description = "Role Name is not valid!"
                });
            } else {
                var owner = await manager.Roles.FirstOrDefaultAsync(x => x.ApplicationId == role.ApplicationId && x.NormalizedName == roleName);

                if (owner != null && !string.Equals(manager.GetRoleIdAsync(owner), manager.GetRoleIdAsync(role))) {
                    return IdentityResult.Failed(new IdentityError {
                        Code = "DuplicateRoleName",
                        Description = "this role already exist in this App!"
                    });
                }
            }
            return IdentityResult.Success;
        }
    }
}
