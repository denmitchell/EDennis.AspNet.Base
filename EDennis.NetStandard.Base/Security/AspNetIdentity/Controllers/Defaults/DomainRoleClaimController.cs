using Microsoft.Extensions.Logging;

namespace EDennis.NetStandard.Base {

    public abstract class DomainRoleClaimController : DomainRoleClaimController<DomainIdentityDbContext, DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        protected DomainRoleClaimController(DbContextProvider<DomainIdentityDbContext> provider, ILogger<QueryController<DomainIdentityDbContext, DomainRoleClaim>> logger) : base(provider, logger) {
        }
    }
}
