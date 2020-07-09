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
    /// <typeparam name="TRole">DomainRole or subclass</typeparam>
    public class DomainRoleManager<TUser,TRole,TContext> : RoleManager<TRole>
        where TUser : DomainUser, new()
        where TRole : DomainRole
        where TContext : DomainIdentityDbContext<TUser, TRole> {
        public DomainRoleManager(IRoleStore<TRole> store, IEnumerable<IRoleValidator<TRole>> roleValidators, 
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, ILogger<RoleManager<TRole>> logger) 
            : base(store, roleValidators, keyNormalizer, errors, logger) {
        }

        public override async Task<IdentityResult> CreateAsync(TRole role) {

            if (!(Store is RoleStore<TRole, TContext, Guid> store))
                throw new Exception("Cannot use DomainRoleManager.CreateAsync without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");

            var db = store.Context.Database;
            var cxn = db.GetDbConnection();
            var cmd = cxn.CreateCommand();
            cmd.CommandText = "di.DomainRoleManager.Create";
            cmd.CommandType = CommandType.StoredProcedure;

            if (db.CurrentTransaction != null)
                cmd.Transaction = db.CurrentTransaction.GetDbTransaction();
            
            cmd.Parameters.Add(role.Id == default ? CombGuid.Create() : role.Id);
            cmd.Parameters.Add(role.ApplicationId);
            cmd.Parameters.Add(role.OrganizationId);
            cmd.Parameters.Add(role.Title);

            await cmd.ExecuteNonQueryAsync();

            return IdentityResult.Success;
        }



        public override async Task<IdentityResult> UpdateAsync(TRole role) {
            if (!(Store is RoleStore<TRole, TContext, Guid> store))
                throw new Exception("Cannot use DomainRoleManager.CreateAsync without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");

            var db = store.Context.Database;
            var cxn = db.GetDbConnection();
            var cmd = cxn.CreateCommand();
            cmd.CommandText = "di.DomainRoleManager.Update";
            cmd.CommandType = CommandType.StoredProcedure;

            if (db.CurrentTransaction != null)
                cmd.Transaction = db.CurrentTransaction.GetDbTransaction();

            cmd.Parameters.Add(role.Id);
            cmd.Parameters.Add(role.ApplicationId);
            cmd.Parameters.Add(role.OrganizationId);
            cmd.Parameters.Add(role.Title);

            await cmd.ExecuteNonQueryAsync();

            return IdentityResult.Success;
        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<IdentityResult> SetRoleNameAsync(TRole role, string name) {
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            return IdentityResult.Failed(new IdentityError() { 
                Code =  "SetRoleNameAsync", 
                Description="Invalid attempt to directly set Role.Name property.  Must instead set Role.RoleName and either Role.ApplicationId or Role.OrganizationId." 
            });
        }



        public virtual async Task<IEnumerable<TRole>> GetRolesForApplicationAsync(string applicationName) {


            if (!(Store is RoleStore<TRole, TContext, Guid> store))
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


        public virtual async Task<IEnumerable<TRole>> GetRolesForApplicationAsync(Guid applicationId) {

            if(!SupportsQueryableRoles)
                throw new Exception("Cannot use DomainRoleManager.GetRolesForApplicationAsync(int applicationId) without Queryable Roles.");

            var qry = Roles
                .Where(r => r.ApplicationId == applicationId)
                .AsNoTracking();

            return await qry.ToListAsync();
        }


        public virtual async Task<IEnumerable<TRole>> GetRolesForOrganizationAsync(string organizationName) {

            if (!(Store is RoleStore<TRole, TContext, Guid> store))
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


        public virtual async Task<IEnumerable<TRole>> GetRolesForOrganizationAsync(Guid organizationId) {

            if (!SupportsQueryableRoles)
                throw new Exception("Cannot use DomainRoleManager.GetRolesForOrganizationAsync(int organizationId) without Queryable Roles.");

            var qry = Roles
                .Where(r => r.OrganizationId == organizationId)
                .AsNoTracking();

            return await qry.ToListAsync();
        }



        public virtual async Task<IEnumerable<Claim>> GetClaimsAsync(IEnumerable<TRole> roles) {

            if (!(Store is RoleStore<TRole, TContext, Guid> store))
                throw new Exception("Cannot use DomainRoleManager.GetClaimsAsync(IEnumerable<TRole> roles) without Microsoft.AspNetCore.Identity.EntityFrameworkCore.RoleStore<TRole> where TRole : DomainRole.");

            if (!store.Context.Database.ProviderName.Contains("SqlServer"))
                throw new Exception("Cannot use DomainRoleManager.GetClaimsAsync(IEnumerable<TRole> roles) without SqlServer provider");

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
