using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Blazor Api Client for communicating with an
    /// internal controller via secure HttpClient.
    /// Note: Uses integrated Identity Server and Microsoft-generated
    /// DI for HttpClientFactory/HttpClient in the Blazor Client Program class.  
    /// Just extend this class and AddScoped<MyExtendedBlazorQueryApiClient>()
    public abstract class BlazorQueryApiClient<TEntity> : IQueryApiClient<TEntity> where TEntity : class {

        public HttpClient HttpClient { get; }

        public BlazorQueryApiClient(HttpClient client) {
            HttpClient = client;
        }


        public virtual void AdjustQuery(ref IQueryable<TEntity> query) { }


        public virtual ObjectResult<List<TEntity>> GetAll() {
            var result = new ObjectResult<List<TEntity>>(default);
            try {
                result = HttpClient.Get<List<TEntity>>($"{ControllerPath}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }


        public virtual async Task<ObjectResult<List<TEntity>>> GetAllAsync() {
            var result = new ObjectResult<List<TEntity>>(default);
            try {
                result = await HttpClient.GetAsync<List<TEntity>>($"{ControllerPath}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }


        public ObjectResult<DeserializableLoadResult<TEntity>> GetWithDevExtreme(string select, string include, string sort, string filter, int skip, int take, string totalSummary, string group, string groupSummary, bool requireTotalCount, bool requireGroupCount) {
            var result = new ObjectResult<DeserializableLoadResult<TEntity>>(default);
            try {
                var qString = BuildDevExtremeQueryString(select, include, sort, filter, skip, take, totalSummary, group, groupSummary, requireTotalCount, requireGroupCount);
                result = HttpClient.Get<DeserializableLoadResult<TEntity>>($"{ControllerPath}/devextreme{qString}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }

        public async Task<ObjectResult<DeserializableLoadResult<TEntity>>> GetWithDevExtremeAsync(string select, string include, string sort, string filter, int skip, int take, string totalSummary, string group, string groupSummary, bool requireTotalCount, bool requireGroupCount) {
            var result = new ObjectResult<DeserializableLoadResult<TEntity>>(default);
            try {
                var qString = BuildDevExtremeQueryString(select, include, sort, filter, skip, take, totalSummary, group, groupSummary, requireTotalCount, requireGroupCount);
                result = await HttpClient.GetAsync<DeserializableLoadResult<TEntity>>($"{ControllerPath}/devextreme/async{qString}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }


        public ObjectResult<DynamicLinqResult<TEntity>> GetWithDynamicLinq(string where = null, string orderBy = null, string select = null, string include = null, int? skip = null, int? take = null, int? totalRecords = null) {
            var result = new ObjectResult<DynamicLinqResult<TEntity>>(default);
            try {
                var qString = BuildDynamicLinqQueryString(where, orderBy, select, include, skip, take, totalRecords);
                result = HttpClient.Get<DynamicLinqResult<TEntity>>($"{ControllerPath}/linq{qString}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }


        public async Task<ObjectResult<DynamicLinqResult<TEntity>>> GetWithDynamicLinqAsync(string where = null, string orderBy = null, string select = null, string include = null, int? skip = null, int? take = null, int? totalRecords = null) {
            var result = new ObjectResult<DynamicLinqResult<TEntity>>(default);
            try {
                var qString = BuildDynamicLinqQueryString(where, orderBy, select, include, skip, take, totalRecords);
                var url = $"{ControllerPath}/linq/async{qString}";
                result = await HttpClient.GetAsync<DynamicLinqResult<TEntity>>($"{ControllerPath}/linq/async{qString}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }

        public IEnumerable<TEntity> GetWithOData(string select, string orderBy, string filter, string expand, int skip, int top) {
            var result = new List<TEntity>();
            try {
                throw new System.NotImplementedException();
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }

        public virtual string ControllerPath {
            get {
                return $"{ApiConstants.ROUTE_PREFIX}{ControllerName}";
            }
        }

        public abstract string ControllerName { get; }


        private static string BuildDevExtremeQueryString(string select, string include, string sort, string filter, int skip, int take, string totalSummary, string group, string groupSummary, bool requireTotalCount, bool requireGroupCount) {
            var list = new List<string>();
            if (select != default)
                list.Add($"select={select}");
            if (include != default)
                list.Add($"include={include}");
            if (sort != default)
                list.Add($"sort={sort}");
            list.Add($"filter={filter}");
            if (skip != default)
                list.Add($"skip={skip}");
            if (take != default)
                list.Add($"take={take}");
            if (totalSummary != default)
                list.Add($"totalSummary={totalSummary}");
            if (group != default)
                list.Add($"group={group}");
            if (groupSummary != default)
                list.Add($"groupSummary={groupSummary}");
            if (requireTotalCount)
                list.Add("requireTotalCount=true");
            if (requireGroupCount)
                list.Add("requireGroupCount=true");

            if (list.Count == 0)
                return "";

            return "?" + string.Join('&', list);
        }


        private static string BuildDynamicLinqQueryString(string where = null, string orderBy = null, string select = null, string include = null, int? skip = null, int? take = null, int? totalRecords = null) {
            var list = new List<string>();
            if (where != default)
                list.Add($"where={where}");
            if (select != default)
                list.Add($"select={select}");
            if (include != default)
                list.Add($"include={include}");
            if (orderBy != default)
                list.Add($"orderBy={orderBy}");
            if (skip != default)
                list.Add($"skip={skip}");
            if (take != default)
                list.Add($"take={take}");
            if (totalRecords != default)
                list.Add($"totalRecords={totalRecords}");

            if (list.Count == 0)
                return "";

            return "?" + string.Join('&', list);
        }

        public string ClientName { get; }
    }
}
