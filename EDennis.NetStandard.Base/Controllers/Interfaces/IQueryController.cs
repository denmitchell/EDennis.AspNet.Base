using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public interface IQueryController<TEntity> where TEntity : class {
        void AdjustQuery(ref IQueryable<TEntity> query);

        IActionResult GetAll();
        Task<IActionResult> GetAllAsync();
        IActionResult GetWithDevExtreme([FromQuery] string select, [FromQuery] string sort, [FromQuery] string filter, [FromQuery] int skip, [FromQuery] int take, [FromQuery] string totalSummary, [FromQuery] string group, [FromQuery] string groupSummary);
        Task<IActionResult> GetWithDevExtremeAsync([FromQuery] string select, [FromQuery] string sort, [FromQuery] string filter, [FromQuery] int skip, [FromQuery] int take, [FromQuery] string totalSummary, [FromQuery] string group, [FromQuery] string groupSummary);
        IActionResult GetWithDynamicLinq([FromQuery] string where = null, [FromQuery] string orderBy = null, [FromQuery] string select = null, [FromQuery] string include = null, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] int? totalRecords = null);
        Task<IActionResult> GetWithDynamicLinqAsync([FromQuery] string where = null, [FromQuery] string orderBy = null, [FromQuery] string select = null, [FromQuery] string include = null, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] int? totalRecords = null);
        IEnumerable<TEntity> GetWithOData([FromQuery] string select, [FromQuery] string orderBy, [FromQuery] string filter, [FromQuery] string expand, [FromQuery] int skip, [FromQuery] int top);
    }
}