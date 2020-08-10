using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class UserTokenController : DomainUserTokenController {
        public UserTokenController(DbContextProvider<DomainIdentityDbContext> provider,
            ILogger<QueryController<DomainIdentityDbContext, DomainUserToken>> logger) : base(provider, logger) {
        }
    }
}
