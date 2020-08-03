using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class RoleController : IdpRoleController {
        public RoleController(DomainRoleRepo repo) : base(repo) {
        }
    }
}
