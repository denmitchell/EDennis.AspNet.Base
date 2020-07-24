using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework;
using EDennis.AspNet.Base.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer.Controllers {

    [Authorize(Policy = "AdministerIDP")]
    public class ApplicationController : IdpApplicationController {
        public ApplicationController(DbContextProvider<DomainIdentityDbContext> provider, ILogger<QueryController<DomainIdentityDbContext, DomainApplication>> logger) : base(provider, logger) {
        }
    }
}
