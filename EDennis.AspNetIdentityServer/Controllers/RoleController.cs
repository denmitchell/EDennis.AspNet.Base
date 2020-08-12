using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class RoleController : DomainRoleController<DomainRole> {
        public RoleController(DomainRoleManager<DomainRole> roleManager) : base(roleManager) {
        }
    }
}
