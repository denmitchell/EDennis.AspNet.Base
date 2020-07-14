using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework;
using Hr.PersonApi.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Hr.StateApi.Controllers {
    public class StateController : CrudController<HrContext, State> {
        public StateController(DbContextProvider<HrContext> provider, ILogger<QueryController<HrContext, State>> logger)
            : base(provider, logger) { }

        public override IQueryable<State> Find(string pathParameter)
            => _dbContext.Set<State>().Where(p => p.Code == pathParameter);
    }
}

