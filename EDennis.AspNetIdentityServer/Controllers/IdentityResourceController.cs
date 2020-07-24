using EDennis.AspNet.Base.Security;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer.Controllers {

    [Authorize(Policy = "AdministerIDP")]
    public class IdentityResourceController : IdpIdentityResourceController<ConfigurationDbContext> {
        public IdentityResourceController(ConfigurationDbContext dbContext) : base(dbContext) {
        }
    }
}
