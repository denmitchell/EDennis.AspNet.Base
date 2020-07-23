using EDennis.AspNet.Base.Security;

namespace EDennis.AspNetIdentityServer.Controllers {
    public class RoleController : IdpRoleController {
        public RoleController(DomainRoleRepo repo) : base(repo) {
        }
    }
}
