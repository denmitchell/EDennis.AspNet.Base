using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Api Client for communicating with an
    /// internal controller via secure HttpClient.
    /// Note: setup the DependencyInjection to include:
    /// <list type="bullet">
    /// <item>AddSecureTokenService<TTokenService>(Configuration,"Security:ClientCredentials")</item>
    /// <item>AddApiClients(Configuration,"ApiClients");</item>
    /// </list>
    public abstract class QueryApiClient<TEntity> : IQueryApiClient<TEntity> where TEntity : class {

        protected readonly HttpClient _client;
        protected readonly ScopedRequestMessage _scopedRequestMessage;

        public abstract string ClientName { get; }


        public QueryApiClient(IHttpClientFactory clientFactory, ITokenService tokenService,
            ScopedRequestMessage scopedRequestMessage) {
            _client = clientFactory.CreateClient(ClientName);
            tokenService.AssignTokenAsync(_client).Wait();
            _scopedRequestMessage = scopedRequestMessage;
        }


        public virtual void AdjustQuery(ref IQueryable<TEntity> query) { }


        public virtual ObjectResult<List<TEntity>> GetAll() {
            return _client.Get<List<TEntity>>($"{ControllerPath}");
        }


        public virtual async Task<ObjectResult<List<TEntity>>> GetAllAsync() {
            return await _client.GetAsync<List<TEntity>>($"{ControllerPath}");
        }


        public ObjectResult<DeserializableLoadResult<TEntity>> GetWithDevExtreme(string select, string include, string sort, string filter, int skip, int take, string totalSummary, string group, string groupSummary) {
            var qString = BuildDevExtremeQueryString(select, include, sort, filter, skip, take, totalSummary, group, groupSummary);
            _scopedRequestMessage.AddQueryString(qString);
            return _client.Get<DeserializableLoadResult<TEntity>>($"{ControllerPath}/devextreme", _scopedRequestMessage);
        }

        public async Task<ObjectResult<DeserializableLoadResult<TEntity>>> GetWithDevExtremeAsync(string select, string include, string sort, string filter, int skip, int take, string totalSummary, string group, string groupSummary) {
            var qString = BuildDevExtremeQueryString(select, include, sort, filter, skip, take, totalSummary, group, groupSummary);
            _scopedRequestMessage.AddQueryString(qString);
            return await _client.GetAsync<DeserializableLoadResult<TEntity>>($"{ControllerPath}/devextreme/async", _scopedRequestMessage);
        }


        public ObjectResult<DynamicLinqResult<TEntity>> GetWithDynamicLinq(string where = null, string orderBy = null, string select = null, string include = null, int? skip = null, int? take = null, int? totalRecords = null) {
            var qString = BuildDynamicLinqQueryString(where, orderBy, select, include, skip, take, totalRecords);
            _scopedRequestMessage.AddQueryString(qString);
            return _client.Get<DynamicLinqResult<TEntity>>($"{ControllerPath}/linq", _scopedRequestMessage);
        }


        public async Task<ObjectResult<DynamicLinqResult<TEntity>>> GetWithDynamicLinqAsync(string where = null, string orderBy = null, string select = null, string include = null, int? skip = null, int? take = null, int? totalRecords = null) {
            var qString = BuildDynamicLinqQueryString(where, orderBy, select, include, skip, take, totalRecords);
            _scopedRequestMessage.AddQueryString(qString);
            return await _client.GetAsync<DynamicLinqResult<TEntity>>($"{ControllerPath}/linq/async", _scopedRequestMessage);
        }

        public IEnumerable<TEntity> GetWithOData(string select, string orderBy, string filter, string expand, int skip, int top) {
            throw new System.NotImplementedException();
        }

        public virtual string ControllerPath {
            get {
                return $"{ApiConstants.ROUTE_PREFIX}{ControllerName}";
            }
        }

        public abstract string ControllerName { get; }


        private static string BuildDevExtremeQueryString(string select, string include, string sort, string filter, int skip, int take, string totalSummary, string group, string groupSummary) {
            var list = new List<string>();
            if (select != default)
                list.Add($"select={select}");
            if (include != default)
                list.Add($"include={include}");
            if (sort != default)
                list.Add($"sort={sort}");
            if (filter != default)
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

    }
}
