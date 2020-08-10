using Microsoft.Extensions.Logging;

namespace EDennis.NetStandard.Base {

    public abstract class DomainRoleViewController : DomainRoleViewController<DomainIdentityDbContext, DomainRoleView, DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        protected DomainRoleViewController(DbContextProvider<DomainIdentityDbContext> provider, ILogger<QueryController<DomainIdentityDbContext, DomainRoleView>> logger) : base(provider, logger) {
        }
    }
}
