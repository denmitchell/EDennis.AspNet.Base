using Microsoft.Extensions.Logging;

namespace EDennis.NetStandard.Base {

    public abstract class DomainOrganizationController : DomainOrganizationController<DomainIdentityDbContext, DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        protected DomainOrganizationController(DbContextProvider<DomainIdentityDbContext> provider, ILogger<QueryController<DomainIdentityDbContext, DomainOrganization>> logger) : base(provider, logger) {
        }
    }
}
