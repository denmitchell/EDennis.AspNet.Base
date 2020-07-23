using EDennis.AspNet.Base.Security;
using IdentityServer4.EntityFramework.DbContexts;

namespace EDennis.AspNetIdentityServer.Controllers {
    public class ApiResourceController : IdpApiResourceController<ConfigurationDbContext> {
        public ApiResourceController(ConfigurationDbContext dbContext) : base(dbContext) {
        }
    }
}
