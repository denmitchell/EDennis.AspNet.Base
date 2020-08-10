using Microsoft.Extensions.Logging;

namespace EDennis.NetStandard.Base {

    public abstract class DomainUserViewController : DomainUserViewController<DomainIdentityDbContext, DomainUserView, DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        protected DomainUserViewController(DbContextProvider<DomainIdentityDbContext> provider, ILogger<QueryController<DomainIdentityDbContext, DomainUserView>> logger) : base(provider, logger) {
        }
    }
}
