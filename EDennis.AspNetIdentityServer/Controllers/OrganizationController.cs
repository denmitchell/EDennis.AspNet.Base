using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework;
using EDennis.AspNet.Base.Security;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer.Controllers {
    public class OrganizationController : IdpOrganizationController {
        public OrganizationController(DbContextProvider<DomainIdentityDbContext> provider, ILogger<QueryController<DomainIdentityDbContext, DomainOrganization>> logger) : base(provider, logger) {
        }
    }
}
