using Dapper;
using EDennis.AspNet.Base.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// <typeparam name="DomainRole">DomainRole or subclass</typeparam>
    public class DomainRoleManager : RoleManager<DomainRole> {
        public DomainRoleManager(IRoleStore<DomainRole> store, IEnumerable<IRoleValidator<DomainRole>> roleValidators, 
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<DomainRole>> logger) 
            : base(store, roleValidators, keyNormalizer, errors, logger) {
        }


        public virtual async Task<IEnumerable<DomainRole>> GetRolesForApplicationAsync(string applicationName) {


            if (!(Store is RoleStore<DomainRole, DomainIdentityDbContext, Guid> store))
                throw new Exception("Cannot use DomainRoleManager.GetRolesForApplicationAsync(string applicationName) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<DomainRole> where DomainRole : DomainRole.");


            var qry = store.Context.Set<DomainRole>()
                .FromSqlInterpolated($@"
select r.* 
  from AspNetRoles r
  inner join AspNetApplications a
    on a.Id = r.ApplicationId
  where a.Name = {applicationName}
            ").AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<DomainRole>> GetRolesForApplicationAsync(Guid applicationId) {

            if(!SupportsQueryableRoles)
                throw new Exception("Cannot use DomainRoleManager.GetRolesForApplicationAsync(int applicationId) without Queryable Roles.");

            var qry = Roles
                .Where(r => r.ApplicationId == applicationId)
                .AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<DomainRole>> GetRolesForOrganizationAsync(string organizationName) {

            if (!(Store is RoleStore<DomainRole, DomainIdentityDbContext, Guid> store))
                throw new Exception("Cannot use DomainRoleManager.GetRolesForOrganizationAsync(string organizationName) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<DomainRole> where DomainRole : DomainRole.");

            var qry = store.Context.Set<DomainRole>()
                .FromSqlInterpolated($@"
select r.* 
  from AspNetRoles r
  inner join AspNetOrganizations o
    on o.Id = r.OrganizationId
  where o.Name = {organizationName}
            ").AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<DomainRole>> GetRolesForOrganizationAsync(Guid organizationId) {

            if (!SupportsQueryableRoles)
                throw new Exception("Cannot use DomainRoleManager.GetRolesForOrganizationAsync(int organizationId) without Queryable Roles.");

            var qry = Roles
                .Where(r => r.OrganizationId == organizationId)
                .AsNoTracking();

            return await qry.ToListAsync();
        }



        public virtual async Task<IEnumerable<Claim>> GetClaimsAsync(IEnumerable<DomainRole> roles) {

            if (!(Store is RoleStore<DomainRole, DomainIdentityDbContext, Guid> store))
                throw new Exception("Cannot use DomainRoleManager.GetClaimsAsync(IEnumerable<DomainRole> roles) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<DomainRole> where DomainRole : DomainRole.");

            if (!store.Context.Database.ProviderName.Contains("SqlServer"))
                throw new Exception("Cannot use DomainRoleManager.GetClaimsAsync(IEnumerable<DomainRole> roles) without SqlServer provider");

            var db = store.Context.Database;
            var cxn = db.GetDbConnection();
            var param = roles.Select(r => r.Name).ToStringTableTypeParameter();


            var results = await cxn.QueryAsync<ClaimModel>("exec di.DomainRoleManager.GetClaims",
                param: new {
                    RoleNames = roles.Select(r => r.Name).ToStringTableTypeParameter()
                },
                transaction: db.CurrentTransaction?.GetDbTransaction()
                );

            return results.Select(c=>new Claim(c.ClaimType,c.ClaimValue));
        }
    }

}
