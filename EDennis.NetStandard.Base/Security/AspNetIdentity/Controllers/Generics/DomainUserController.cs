using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace EDennis.NetStandard.Base {

    public abstract class DomainUserController<TContext, TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole> :
        CrudController<TContext,TUser>
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


        public DomainUserController(DbContextProvider<TContext> provider, 
            ILogger<QueryController<TContext, TUser>> logger
            ) : base(provider, logger) {
        }


        [NonAction]
        public override IQueryable<TUser> Find(string pathParameter) {
            if (int.TryParse(pathParameter, out int id))
                return _dbContext.Set<TUser>().Where(e => e.Id == id);
            else if (pathParameter.Contains("@"))
                return _dbContext.Set<TUser>().Where(e => e.NormalizedEmail == pathParameter.ToUpper());
            else
                return _dbContext.Set<TUser>().Where(a => a.NormalizedUserName == pathParameter.ToUpper());
        }

    }
}
