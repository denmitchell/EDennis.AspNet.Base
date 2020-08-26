using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {


    public class DomainRoleManager : RoleManager<DomainRole> {
        public DomainRoleManager(IRoleStore<DomainRole> store, 
            IEnumerable<IRoleValidator<DomainRole>> roleValidators, 
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
            ILogger<RoleManager<DomainRole>> logger) 
            : base(store, roleValidators, keyNormalizer, errors, logger) {
        }
    }


    public class DomainRoleManager<TRole> : RoleManager<TRole>
        where TRole : DomainRole {
        public DomainRoleManager(IRoleStore<TRole> store, IEnumerable<IRoleValidator<TRole>> roleValidators, 
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<TRole>> logger) 
            : base(store, roleValidators, keyNormalizer, errors, logger) {
        }

        public virtual async Task<ICollection<TRole>> GetRolesForApplicationAsync(string application) {
            return await Roles.Where(u => u.Application == application).ToListAsync();
        }

        public virtual async Task<TRole> FindByIdAsync(int id) {
            return await Roles.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IdentityResult> AddClaimsAsync(TRole role, IEnumerable<Claim> claims) {
            var errors = new List<IdentityError>();

            foreach (var claim in claims) {
                var result = await AddClaimAsync(role, claim);
                if (!result.Succeeded)
                    errors.AddRange(result.Errors);
            }
            if (errors.Count > 0)
                return IdentityResult.Failed(errors.ToArray());
            else
                return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveClaimsAsync(TRole role, IEnumerable<Claim> claims) {
            var errors = new List<IdentityError>();

            foreach (var claim in claims) {
                var result = await RemoveClaimAsync(role, claim);
                if (!result.Succeeded)
                    errors.AddRange(result.Errors);
            }
            if (errors.Count > 0)
                return IdentityResult.Failed(errors.ToArray());
            else
                return IdentityResult.Success;
        }

    }
}
