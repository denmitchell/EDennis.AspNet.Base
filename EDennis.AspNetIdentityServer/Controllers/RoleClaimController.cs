using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class RoleClaimController : DomainRoleClaimController {
        public RoleClaimController(DbContextProvider<DomainIdentityDbContext> provider,
            ILogger<QueryController<DomainIdentityDbContext, DomainRoleClaim>> logger) : base(provider, logger) {
        }
    }
}
