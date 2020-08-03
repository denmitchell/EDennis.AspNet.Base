using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class OrganizationController : IdpOrganizationController {
        public OrganizationController(DomainOrganizationRepo repo) : base(repo) {
        }
    }
}
