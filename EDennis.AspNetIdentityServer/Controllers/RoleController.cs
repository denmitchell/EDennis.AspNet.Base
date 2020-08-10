using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class RoleController : DomainRoleController {
        public RoleController(DbContextProvider<DomainIdentityDbContext> provider,
            ILogger<QueryController<DomainIdentityDbContext, DomainRole>> logger) : base(provider, logger) {
        }
    }
}
