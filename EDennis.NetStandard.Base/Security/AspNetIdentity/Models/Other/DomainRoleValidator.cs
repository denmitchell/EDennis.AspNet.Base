using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    public class DomainRoleValidator : RoleValidator<DomainRole> { }


    /// <summary>
    /// Ensures that RoleManager requires uniqueness of combination of 
    ///   ApplicationId and NormalizedRoleName, rather than just NormalizedRoleName
    ///   
    /// NOTE: In Startup.ConfigureServices:
    ///         services.AddDefaultIdentity<DomainUser>(options => options.SignIn.RequireConfirmedAccount = true)
    ///            .AddEntityFrameworkStores<DomainIdentityDbContext>()
    ///            .AddRoleValidator<DomainRoleValidator<TRole>(); //add this line
    ///   
    /// Adapted from https://stackoverflow.com/a/59466933
    /// </summary>
    /// <typeparam name="TRole">Role Type</typeparam>
    public class DomainRoleValidator<TRole> : RoleValidator<TRole>
    where TRole : DomainRole {

        public override async Task<IdentityResult> ValidateAsync(RoleManager<TRole> manager, TRole role) {
            var roleName = await manager.GetRoleNameAsync(role);
            if (string.IsNullOrWhiteSpace(roleName)) {
                return IdentityResult.Failed(new IdentityError {
                    Code = "RoleNameIsNotValid",
                    Description = "Role Name is not valid!"
                });
            } else {
                var owner = await manager.Roles.FirstOrDefaultAsync(x => x.Application == role.Application && x.NormalizedName == roleName);

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
