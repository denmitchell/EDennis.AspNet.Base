using EDennis.AspNet.Base.Security;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer.Controllers {

    [Authorize(Policy = "AdministerIDP")]
    public class ClientController : IdpClientController<ConfigurationDbContext> {
        public ClientController(ConfigurationDbContext dbContext) : base(dbContext) {
        }
    }
}
