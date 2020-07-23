using EDennis.AspNet.Base.Security;
using IdentityServer4.EntityFramework.DbContexts;


namespace EDennis.AspNetIdentityServer.Controllers {
    public class ClientController : IdpClientController<ConfigurationDbContext> {
        public ClientController(ConfigurationDbContext dbContext) : base(dbContext) {
        }
    }
}
