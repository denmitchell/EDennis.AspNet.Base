using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
    ///            .AddRoleValidator<DomainRoleValidator<TRole>>(); //add this line
    ///   
    /// Adapted from https://stackoverflow.com/a/59466933
    /// </summary>
    /// <typeparam name="TRole">Role Type</typeparam>
    public class DomainRoleValidator<TRole> : RoleValidator<TRole>
    where TRole : DomainRole {

        private readonly IdentityErrorDescriber _errorDescriber;

        public DomainRoleValidator(IdentityErrorDescriber errorDescriber) {
            _errorDescriber = errorDescriber;
        }


        public override async Task<IdentityResult> ValidateAsync(RoleManager<TRole> manager, TRole role) {
            var errors = new List<IdentityError>();
            if (manager == null) {
                throw new ArgumentNullException(nameof(manager));
            }
            var roleName = await manager.GetRoleNameAsync(role);
            if (string.IsNullOrWhiteSpace(roleName) && string.IsNullOrWhiteSpace(role.NormalizedName))
                errors.Add(_errorDescriber.InvalidRoleName(roleName));
            if (string.IsNullOrWhiteSpace(role.Application) && string.IsNullOrWhiteSpace(role.NormalizedName))
                errors.Add(_errorDescriber.InvalidApplicationName(role.Application));
            if (!string.IsNullOrWhiteSpace(roleName) && string.IsNullOrWhiteSpace(role.Application)) {
                var owner = await manager.FindByNameAsync(role.NormalizedName);
                if (owner != null &&
                    !string.Equals(await manager.GetRoleIdAsync(owner), await manager.GetRoleIdAsync(role))) {
                    errors.Add(_errorDescriber.DuplicateRoleName(role.NormalizedName));
                }
            }
            if (errors.Count > 0)
                return IdentityResult.Failed(errors.ToArray());
            else
                return IdentityResult.Success;
        }

    }

}
