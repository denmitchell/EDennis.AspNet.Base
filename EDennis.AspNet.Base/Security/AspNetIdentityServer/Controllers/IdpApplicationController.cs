using EDennis.AspNet.Base.EntityFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EDennis.AspNet.Base.Security {

    [Authorize(Policy = "AdministerIDP")]
    [Route("api/[controller]")]
    [ApiController]
    public abstract class IdpApplicationController : CrudController<DomainIdentityDbContext, DomainApplication> {
        public IdpApplicationController(DbContextProvider<DomainIdentityDbContext> provider, 
            ILogger<QueryController<DomainIdentityDbContext, DomainApplication>> logger) 
            : base(provider, logger) { }

        public override IQueryable<DomainApplication> Find(string pathParameter)
            => _dbContext.Applications.Where(a=>a.Name == pathParameter);
    }
}
