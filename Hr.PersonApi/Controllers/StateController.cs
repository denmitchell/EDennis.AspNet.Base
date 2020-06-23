using EDennis.AspNet.Base;
using Hr.PersonApi.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Hr.StateApi.Controllers {
    public class StateController : CrudController<HrContext, State> {
        public StateController(HrContext context, ILogger<QueryController<HrContext, State>> logger)
            : base(context, logger) { }

        public override IQueryable<State> Find(string pathParameter)
            => _dbContext.Set<State>().Where(p => p.Code == pathParameter);
    }
}

