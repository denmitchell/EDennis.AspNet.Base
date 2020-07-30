using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base.Security;

namespace EDennis.AspNetIdentityServer.Controllers {

    [Authorize(Policy = "AdministerIDP")]
    public class ApplicationController : IdpApplicationController {
        public ApplicationController(DomainApplicationRepo repo) : base(repo) {
        }
    }
}
