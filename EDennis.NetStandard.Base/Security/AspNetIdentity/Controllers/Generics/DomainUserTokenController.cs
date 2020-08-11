using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace EDennis.NetStandard.Base {

    public abstract class DomainUserTokenController<TContext, TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole> :
        CrudController<TContext, TUserToken>
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

        public DomainUserTokenController(DbContextProvider<TContext> provider, 
            ILogger<QueryController<TContext, TUserToken>> logger
            ) : base(provider, logger) {
        }


        [NonAction]
        public override IQueryable<TUserToken> Find(string pathParameter) {
            try { 
                var parms = pathParameter.Split("/");
                var userId = int.Parse(parms[0]);
                var loginProvider = parms[1];
                var name = parms[2];
                return _dbContext.Set<TUserToken>().Where(e => e.UserId == userId && e.LoginProvider == loginProvider && e.Name == name);
            } catch (Exception ex) {
                var aex = new ArgumentException($"Could not parse {pathParameter} into userId, loginProvider and name: {ex.Message}");
                _logger.LogError(aex, aex.Message);
                throw aex;
            }
        }

    }
}
