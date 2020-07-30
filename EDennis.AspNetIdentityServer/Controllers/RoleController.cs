using EDennis.NetStandard.Base.Security;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer.Controllers {

    [Authorize(Policy = "AdministerIDP")]
    public class RoleController : IdpRoleController {
        public RoleController(DomainRoleRepo repo) : base(repo) {
        }
    }
}
