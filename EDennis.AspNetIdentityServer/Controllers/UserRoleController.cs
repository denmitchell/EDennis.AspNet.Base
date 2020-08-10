using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class UserRoleController : DomainUserRoleController {
        public UserRoleController(DbContextProvider<DomainIdentityDbContext> provider,
            ILogger<QueryController<DomainIdentityDbContext, DomainUserRole>> logger) : base(provider, logger) {
        }
    }
}
