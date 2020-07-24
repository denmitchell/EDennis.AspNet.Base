using EDennis.AspNet.Base.Security;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer.Controllers {

    [Authorize(Policy = "AdministerIDP")]
    public class ApiResourceController : IdpApiResourceController<ConfigurationDbContext> {
        public ApiResourceController(ConfigurationDbContext dbContext) : base(dbContext) {
        }
    }
}
