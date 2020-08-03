using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class UserController : IdpUserController {
        public UserController(DomainUserRepo repo) : base(repo) { }
    }
}
