using System.Linq;
using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace EDennis.NetStandard.Base {


    public abstract class DomainOrganizationController<TContext, TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole> :
        CrudController<TContext, TOrganization>
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

        public DomainOrganizationController(DbContextProvider<TContext> provider, 
            ILogger<QueryController<TContext, TOrganization>> logger
            ) : base(provider, logger) {
        }


        [NonAction]
        public override IQueryable<TOrganization> Find(string pathParameter) {
            if (int.TryParse(pathParameter, out int id)) {
                return _dbContext.Set<TOrganization>().Where(e => e.Id == id);
            } else
                return _dbContext.Set<TOrganization>().Where(a => a.Name == pathParameter);
        }

    }
}
