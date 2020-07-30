using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EDennis.AspNet.Base.Security {


    //[Authorize(Policy = "AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public class IdpBaseController : ControllerBase {

        [NonAction]
        protected string GetSysUser() {
            var sysUser = HttpContext.User.Claims
                .OrderByDescending(c => c.Type)
                .FirstOrDefault(c => c.Type == "name"
                    || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                    || c.Type == "email"
                    || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email"
                    || c.Type == "client_id")
                ?.Value;
            return sysUser;
        }

    }
}
