using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public interface IQueryApiClient<TEntity> where TEntity : class {
        string ControllerName { get; }
        string ControllerPath { get; }

        string ClientName { get; }

        void AdjustQuery(ref IQueryable<TEntity> query);
        ObjectResult<List<TEntity>> GetAll();
        Task<ObjectResult<List<TEntity>>> GetAllAsync();
        ObjectResult<DeserializableLoadResult<TEntity>> GetWithDevExtreme(string select, string include, string sort, string filter, int skip, int take, string totalSummary, string group, string groupSummary);
        Task<ObjectResult<DeserializableLoadResult<TEntity>>> GetWithDevExtremeAsync(string select, string include, string sort, string filter, int skip, int take, string totalSummary, string group, string groupSummary);
        ObjectResult<DynamicLinqResult<TEntity>> GetWithDynamicLinq(string where = null, string orderBy = null, string select = null, string include = null, int? skip = null, int? take = null, int? totalRecords = null);
        Task<ObjectResult<DynamicLinqResult<TEntity>>> GetWithDynamicLinqAsync(string where = null, string orderBy = null, string select = null, string include = null, int? skip = null, int? take = null, int? totalRecords = null);
        IEnumerable<TEntity> GetWithOData(string select, string orderBy, string filter, string expand, int skip, int top);
    }
}