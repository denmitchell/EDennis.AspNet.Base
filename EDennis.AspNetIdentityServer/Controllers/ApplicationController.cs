using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class ApplicationController : DomainApplicationController {
        public ApplicationController(DbContextProvider<DomainIdentityDbContext> provider, 
            ILogger<QueryController<DomainIdentityDbContext, DomainApplication>> logger) : base(provider, logger) {
        }
    }
}
