using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class UserClaimController : DomainUserClaimController {
        public UserClaimController(DbContextProvider<DomainIdentityDbContext> provider,
            ILogger<QueryController<DomainIdentityDbContext, DomainUserClaim>> logger) : base(provider, logger) {
        }
    }
}
