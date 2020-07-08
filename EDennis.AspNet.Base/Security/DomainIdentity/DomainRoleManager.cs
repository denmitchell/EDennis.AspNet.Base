using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// A repository class for maintaining instances of DomainRole (which inherits from IdentityRole).
    /// NOTE: This class is one of the "Domain Identity" classes (DomainUserManager, 
    /// DomainRoleManager, DomainUser, DomainRole, IdentityApplication, and IdentityOrganization),
    /// which supports centralized management of user security across different applications and
    /// organizations. This is not a multi-tenant architecture, but an architecture that allows
    /// user management across applications and allows user management to be delegated 
    /// to organization admins.
    /// </summary>
    /// <typeparam name="TRole">DomainRole or subclass</typeparam>
    public class DomainRoleManager<TRole> : AspNetRoleManager<TRole>
        where TRole : DomainRole {
        public DomainRoleManager(IRoleStore<TRole> store, IEnumerable<IRoleValidator<TRole>> roleValidators, 
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<TRole>> logger, 
            IHttpContextAccessor contextAccessor) 
            : base(store, roleValidators, keyNormalizer, errors, logger, contextAccessor) {
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
  set Name = r.RoleName + '@' + o.Name + '@' + a.Name 
  from AspNetRoles r
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  inner join AspNetOrganizations o
    on o.Id = r.OrganizationId
  where r.Id = {role.Id}");
            
            else if (role.OrganizationId != default)
                await db.ExecuteSqlInterpolatedAsync($@"
update r 
  set Name = r.RoleName + '@' + o.Name 
  from AspNetRoles r
  inner join AspNetOrganizations o
    on o.Id = r.OrganizationId
  where r.Id = {role.Id}");

            else
                await db.ExecuteSqlInterpolatedAsync($@"
update r 
  set Name = r.RoleName + '@' + a.Name 
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



        public virtual async Task<IEnumerable<Claim>> GetClaimsAsync(IEnumerable<TRole> roles) {

            //simpler, but less efficient (multiple queries)
            //var claims = new List<Claim>();
            //foreach (var role in roles)
            //    claims.AddRange(await GetClaimsAsync(role));

            if (!(Store is RoleStore<TRole> store))
                throw new Exception("Cannot use DomainManager.GetClaimsAsync(IEnumerable<TRole> roles) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");

            var qry = store.Context.Set<IdentityRoleClaim<string>>()
                .FromSqlInterpolated($@"
select r.* 
  from AspNetRoleClaims rc
  inner join AspNetRoles r
     on r.Id = rc.RoleId
  where r.Name In({string.Join(',', roles.Select(r => $"'{r.Name}'"))})
            ").AsNoTracking()
            .Select(rc=>new Claim(rc.ClaimType,rc.ClaimValue));

            return await qry.ToListAsync();
        }
    }

}
