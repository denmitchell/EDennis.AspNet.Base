using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override async Task<IdentityResult> CreateAsync(TRole role) {
            await UpdateNameAsync(role);
            return await base.CreateAsync(role);
        }

        public override async Task<IdentityResult> UpdateAsync(TRole role) {
            await UpdateNameAsync(role);
            return await base.UpdateAsync(role);
        }



        public override async Task<IdentityResult> SetRoleNameAsync(TRole role, string name) {
            var components = name.Split('@');
            if (components.Length == 1) {
                role.ApplicationId = default;
                role.OrganizationId = default;
                role.RoleName = role.Name;
                return await base.SetRoleNameAsync(role, name);
            } else {
                var result = await base.SetRoleNameAsync(role, name);

                if (!result.Succeeded)
                    return result;

                if (!(Store is RoleStore<TRole> store))
                    throw new Exception("Cannot use DomainRoleManager.SetRoleNameAsync without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");

                var db = store.Context.Database;

                if (components.Length == 3)
                    await db.ExecuteSqlInterpolatedAsync($@"
update r 
  set RoleName = {components[0]}, OrganizationId = o.Id, ApplicationId = a.Id
  from AspNetRoles r
  inner join AspNetOrganizations o
    on o.Name = {components[1]}
  inner join AspNetApplications a
    on a.Name = {components[2]}
  where r.Id = {role.Id}");

                if (components.Length == 2)
                    await db.ExecuteSqlInterpolatedAsync($@"
update r 
  set RoleName = {components[0]}, OrganizationId = o.Id, ApplicationId = a.Id
  from AspNetRoles r
  left outer join AspNetOrganizations o
    on o.Name = {components[1]}
  left outer join AspNetApplications a
    on a.Name = {components[1]}
  where r.Id = {role.Id}");

            }

            return IdentityResult.Success;
        }


        public virtual async Task UpdateNameAsync(TRole role) {
            if (role.RoleName == null || (role.OrganizationId == default && role.ApplicationId == default))
                return;

            if (!(Store is RoleStore<TRole> store))
                throw new Exception("Cannot use DomainRoleManager.UpdateNameAsync without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");

            var db = store.Context.Database;

            //use UPDATE SQL to update the Role column from RoleName, OrganizationId, and ApplicationId
            //this approach is taken because there are no navigation properties in AspNetIdentity due
            //to the way that the generic classes are composed.

            if (role.OrganizationId != default && role.ApplicationId != default)
                await db.ExecuteSqlInterpolatedAsync( $@"
update r 
  set Role = r.RoleName + '@' + o.Name + '@' + a.Name 
  from AspNetRoles r
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  inner join AspNetOrganizations o
    on o.Id = r.OrganizationId
  where r.Id = {role.Id}");
            
            else if (role.OrganizationId != default)
                await db.ExecuteSqlInterpolatedAsync($@"
update r 
  set Role = r.RoleName + '@' + o.Name 
  from AspNetRoles r
  inner join AspNetOrganizations o
    on o.Id = r.OrganizationId
  where r.Id = {role.Id}");

            else
                await db.ExecuteSqlInterpolatedAsync($@"
update r 
  set Role = r.RoleName + '@' + a.Name 
  from AspNetRoles r
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  where r.Id = {role.Id}");

        }



        public virtual async Task<IEnumerable<TRole>> GetRolesForApplicationAsync(string applicationName) {

            

            if (!(Store is RoleStore<TRole> store))
                throw new Exception("Cannot use DomainRoleManager.GetRolesForApplicationAsync(string applicationName) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");


            var qry = store.Context.Set<TRole>()
                .FromSqlInterpolated($@"
select r.* 
  from AspNetRoles r
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  where a.Name = {applicationName}
            ").AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<TRole>> GetRolesForApplicationAsync(int applicationId) {

            if(!SupportsQueryableRoles)
                throw new Exception("Cannot use DomainRoleManager.GetRolesForApplicationAsync(int applicationId) without Queryable Roles.");

            var qry = Roles
                .Where(r => r.ApplicationId == applicationId)
                .AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<TRole>> GetRolesForOrganizationAsync(string organizationName) {

            if (!(Store is RoleStore<TRole> store))
                throw new Exception("Cannot use DomainRoleManager.GetRolesForOrganizationAsync(string organizationName) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");

            var qry = store.Context.Set<TRole>()
                .FromSqlInterpolated($@"
select r.* 
  from AspNetRoles r
  inner join AspNetOrganizations o
    on o.Id = r.OrganizationId
  where o.Name = {organizationName}
            ").AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<TRole>> GetRolesForOrganizationAsync(int organizationId) {

            if (!SupportsQueryableRoles)
                throw new Exception("Cannot use DomainRoleManager.GetRolesForOrganizationAsync(int organizationId) without Queryable Roles.");

            var qry = Roles
                .Where(r => r.OrganizationId == organizationId)
                .AsNoTracking();

            return await qry.ToListAsync();
        }




    }

}
