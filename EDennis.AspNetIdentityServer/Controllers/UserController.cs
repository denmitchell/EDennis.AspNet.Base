using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class UserController : DomainUserController<DomainUser, DomainRole> {
        public UserController(DomainUserManager<DomainUser, DomainRole> userManager) : base(userManager) {
        }
    }
}
