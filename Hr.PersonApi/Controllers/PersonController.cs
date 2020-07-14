using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework;
using Hr.PersonApi.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Hr.PersonApi.Controllers {
    public class PersonController : CrudController<HrContext, Person> {
        public PersonController(DbContextProvider<HrContext> provider, ILogger<QueryController<HrContext,Person>> logger) 
            : base(provider, logger) { }

        public override IQueryable<Person> Find(string pathParameter)
            => _dbContext.Set<Person>().Where(p => p.Id == int.Parse(pathParameter));
    }
}
