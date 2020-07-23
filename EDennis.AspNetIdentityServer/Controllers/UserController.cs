using EDennis.AspNet.Base.Security;
using EDennis.AspNetBase.Security;

namespace EDennis.AspNetIdentityServer.Controllers {
    public class UserController : IdpUserController {
        public UserController(DomainUserRepo repo) : base(repo) { }
    }
}
