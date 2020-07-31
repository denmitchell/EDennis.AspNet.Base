using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.Controllers.ProxyControllers {
    public class ProxyQueryController<TEntity> : IQueryController<TEntity>
        where TEntity : class {

        public virtual void AdjustQuery(ref IQueryable<TEntity> query) {
        }

        public IActionResult GetWithDevExtreme([FromQuery] string select, [FromQuery] string sort, [FromQuery] string filter, [FromQuery] int skip, [FromQuery] int take, [FromQuery] string totalSummary, [FromQuery] string group, [FromQuery] string groupSummary) {
            throw new System.NotImplementedException();
        }

        public Task<IActionResult> GetWithDevExtremeAsync([FromQuery] string select, [FromQuery] string sort, [FromQuery] string filter, [FromQuery] int skip, [FromQuery] int take, [FromQuery] string totalSummary, [FromQuery] string group, [FromQuery] string groupSummary) {
            throw new System.NotImplementedException();
        }

        public IActionResult GetWithDynamicLinq([FromQuery] string where = null, [FromQuery] string orderBy = null, [FromQuery] string select = null, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] int? totalRecords = null) {
            throw new System.NotImplementedException();
        }

        public Task<IActionResult> GetWithDynamicLinqAsync([FromQuery] string where = null, [FromQuery] string orderBy = null, [FromQuery] string select = null, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] int? totalRecords = null) {
            throw new System.NotImplementedException();
        }

        public IEnumerable<TEntity> GetWithOData([FromQuery] string select, [FromQuery] string orderBy, [FromQuery] string filter, [FromQuery] string expand, [FromQuery] int skip, [FromQuery] int top) {
            throw new System.NotImplementedException();
        }
    }
}
