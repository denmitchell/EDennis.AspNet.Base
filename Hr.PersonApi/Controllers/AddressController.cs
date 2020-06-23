using EDennis.AspNet.Base;
using Hr.PersonApi.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Hr.AddressApi.Controllers {
    public class AddressController : CrudController<HrContext, Address> {
        public AddressController(HrContext context, ILogger<QueryController<HrContext, Address>> logger)
            : base(context, logger) { }

        public override IQueryable<Address> Find(string pathParameter)
            => _dbContext.Set<Address>().Where(p => p.Id == int.Parse(pathParameter));
    }
}

