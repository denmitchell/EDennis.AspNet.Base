using EDennis.AspNet.Base.EntityFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EDennis.AspNet.Base.Security {

    [Authorize(Policy = "AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class IdpOrganizationController : CrudController<DomainIdentityDbContext, DomainOrganization> {
        public IdpOrganizationController(DbContextProvider<DomainIdentityDbContext> provider,
            ILogger<QueryController<DomainIdentityDbContext, DomainOrganization>> logger)
            : base(provider, logger) { }

        public override IQueryable<DomainOrganization> Find(string pathParameter)
            => _dbContext.Organizations.Where(a => a.Name == pathParameter);
    }
}
