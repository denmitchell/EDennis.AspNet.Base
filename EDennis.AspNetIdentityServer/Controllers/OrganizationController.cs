using Microsoft.AspNetCore.Authorization;
using EDennis.NetStandard.Base;
using Microsoft.Extensions.Logging;

namespace EDennis.AspNetIdentityServer {

    [Authorize(Policy = "AdministerIDP")]
    public class OrganizationController : DomainOrganizationController<DomainIdentityDbContext,DomainUser,DomainRole> {
        public OrganizationController(DbContextProvider<DomainIdentityDbContext> provider,
            ILogger<QueryController<DomainIdentityDbContext, DomainOrganization>> logger) : base(provider, logger) {
        }
    }
}
