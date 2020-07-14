using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework;
using Hr.PersonApi.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Hr.AddressApi.Controllers {
    public class AddressController : CrudController<HrContext, Address> {
        public AddressController(DbContextProvider<HrContext> provider, ILogger<QueryController<HrContext, Address>> logger)
            : base(provider, logger) { }

        public override IQueryable<Address> Find(string pathParameter)
            => _dbContext.Set<Address>().Where(p => p.Id == int.Parse(pathParameter));
    }
}

