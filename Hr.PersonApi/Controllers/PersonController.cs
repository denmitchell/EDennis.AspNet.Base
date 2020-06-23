using EDennis.AspNet.Base;
using Hr.PersonApi.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Hr.PersonApi.Controllers {
    public class PersonController : CrudController<HrContext, Person> {
        public PersonController(HrContext context, ILogger<QueryController<HrContext,Person>> logger) 
            : base(context, logger) { }

        public override IQueryable<Person> Find(string pathParameter)
            => _dbContext.Set<Person>().Where(p => p.Id == int.Parse(pathParameter));
    }
}
