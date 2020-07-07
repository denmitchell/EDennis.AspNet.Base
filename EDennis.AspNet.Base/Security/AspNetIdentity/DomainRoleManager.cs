using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class DomainRoleManager<TRole> : RoleManager<TRole>
        where TRole : DomainRole {

        public DomainRoleManager(RoleStore<TRole> store,
            IEnumerable<IRoleValidator<TRole>> roleValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<RoleManager<TRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger) {
        }

        public override Task<IdentityResult> CreateAsync(TRole role) {
            UpdateName(role);
            return base.CreateAsync(role);
        }

        public override Task<IdentityResult> UpdateAsync(TRole role) {
            UpdateName(role);
            return base.UpdateAsync(role);
        }

        protected override Task<IdentityResult> UpdateRoleAsync(TRole role) {
            UpdateName(role);
            return base.UpdateRoleAsync(role);
        }


        public virtual void UpdateName(TRole role) {
            if (role.RoleName == null || (role.OrganizationId == default && role.ApplicationId == default))
                return;

            if (!(Store is RoleStore<TRole> store))
                throw new Exception("Cannot use DomainRoleManager without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");

            var db = store.Context.Database;

            //use UPDATE SQL to update the Role column from RoleName, OrganizationId, and ApplicationId
            //this approach is taken because there are no navigation properties in AspNetIdentity due
            //to the way that the generic classes are composed.

            if (role.OrganizationId != default && role.ApplicationId != default)
                db.ExecuteSqlInterpolated( $@"
update r 
  set Role = r.RoleName + '@' + o.Name + '@' + a.Name 
  from AspNetRoles r
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  inner join AspNetOrganizations o
    on o.Id = r.OrganizationId
  where r.Id = {role.Id}");
            
            else if (role.OrganizationId != default)
                db.ExecuteSqlInterpolated($@"
update r 
  set Role = r.RoleName + '@' + o.Name 
  from AspNetRoles r
  inner join AspNetOrganizations o
    on o.Id = r.OrganizationId
  where r.Id = {role.Id}");

            else
                db.ExecuteSqlInterpolated($@"
update r 
  set Role = r.RoleName + '@' + a.Name 
  from AspNetRoles r
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  where r.Id = {role.Id}");

        }

    }

}
