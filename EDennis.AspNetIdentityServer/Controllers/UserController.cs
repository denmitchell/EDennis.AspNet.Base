using EDennis.AspNet.Base.Security;
using EDennis.AspNetBase.Security;
using EDennis.NetStandard.Base.Security;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer.Controllers {

    [Authorize(Policy = "AdministerIDP")]
    public class UserController : IdpUserController {
        public UserController(DomainUserRepo repo) : base(repo) { }
    }
}
