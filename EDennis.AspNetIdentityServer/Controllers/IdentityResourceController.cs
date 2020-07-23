using EDennis.AspNet.Base.Security;
using IdentityServer4.EntityFramework.DbContexts;

namespace EDennis.AspNetIdentityServer.Controllers {
    public class IdentityResourceController : IdpIdentityResourceController<ConfigurationDbContext> {
        public IdentityResourceController(ConfigurationDbContext dbContext) : base(dbContext) {
        }
    }
}
