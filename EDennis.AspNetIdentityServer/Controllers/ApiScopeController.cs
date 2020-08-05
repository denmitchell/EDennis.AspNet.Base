using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authorization;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class ApiScopeController : IdpApiScopeController<ConfigurationDbContext> {
        public ApiScopeController(ConfigurationDbContext dbContext) : base(dbContext) {
        }
    }
}
