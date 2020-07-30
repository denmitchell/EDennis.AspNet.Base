using EDennis.NetStandard.Base.Security;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer.Controllers {

    [Authorize(Policy = "AdministerIDP")]
    public class OrganizationController : IdpOrganizationController {
        public OrganizationController(DomainOrganizationRepo repo) : base(repo) {
        }
    }
}
