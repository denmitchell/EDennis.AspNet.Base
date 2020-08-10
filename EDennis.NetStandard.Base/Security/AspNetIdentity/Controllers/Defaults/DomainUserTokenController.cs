using Microsoft.Extensions.Logging;

namespace EDennis.NetStandard.Base {

    public abstract class DomainUserTokenController : DomainUserTokenController<DomainIdentityDbContext, DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        protected DomainUserTokenController(DbContextProvider<DomainIdentityDbContext> provider, ILogger<QueryController<DomainIdentityDbContext, DomainUserToken>> logger) : base(provider, logger) {
        }
    }
}
