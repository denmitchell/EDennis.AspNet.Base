using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace EDennis.NetStandard.Base {

    public abstract class DomainUserRoleController<TContext, TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole> :
        CrudController<TContext, TUserRole>
        where TContext : DomainIdentityDbContext<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>
        where TUser : DomainUser<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TOrganization : DomainOrganization<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserClaim : DomainUserClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserLogin : DomainUserLogin<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserToken : DomainUserToken<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRole : DomainRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TApplication : DomainApplication<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRoleClaim : DomainRoleClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserRole : DomainUserRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new() {

        public DomainUserRoleController(DbContextProvider<TContext> provider, 
            ILogger<QueryController<TContext, TUserRole>> logger
            ) : base(provider, logger) {
        }


        [NonAction]
        public override IQueryable<TUserRole> Find(string pathParameter) {
            try { 
            var parms = pathParameter.Split("/");
            var userId = Guid.Parse(parms[0]);
            var roleId = Guid.Parse(parms[1]);
                return _dbContext.Set<TUserRole>().Where(e => e.UserId == userId && e.RoleId == roleId);
            } catch (Exception ex) {
                var aex = new ArgumentException($"Could not parse {pathParameter} into userId and roleId: {ex.Message}");
                _logger.LogError(aex, aex.Message);
                throw aex;
            }
        }

    }
}
