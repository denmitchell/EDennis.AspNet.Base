using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public abstract class QueryApiClient<TEntity>
        where TEntity : class {

        protected readonly HttpClient _client;


        public QueryApiClient(IHttpClientFactory clientFactory, ITokenService tokenService) {
            _client = clientFactory.CreateClient(GetType().Name);
            tokenService.AssignTokenAsync(_client).Wait();
        }

        [NonAction]
        public virtual void AdjustQuery(ref IQueryable<TEntity> query) { }


        public IActionResult GetWithDevExtreme(HttpRequest request, [FromQuery] string select, [FromQuery] string sort, [FromQuery] string filter, [FromQuery] int skip, [FromQuery] int take, [FromQuery] string totalSummary, [FromQuery] string group, [FromQuery] string groupSummary) {
            return _client.Forward<DeserializableLoadResult<TEntity>>(request, $"{ControllerPath}/devextreme");
        }

        public async Task<IActionResult> GetWithDevExtremeAsync(HttpRequest request, [FromQuery] string select, [FromQuery] string sort, [FromQuery] string filter, [FromQuery] int skip, [FromQuery] int take, [FromQuery] string totalSummary, [FromQuery] string group, [FromQuery] string groupSummary) {
            return await _client.ForwardAsync<DeserializableLoadResult<TEntity>>(request, $"{ControllerPath}/devextreme/async");
        }


        public IActionResult GetWithDynamicLinq(HttpRequest request, [FromQuery] string where = null, [FromQuery] string orderBy = null, [FromQuery] string select = null, [FromQuery] string include = null, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] int? totalRecords = null) {
            return _client.Forward<TEntity>(request, $"{ControllerPath}/linq");
        }


        public async Task<IActionResult> GetWithDynamicLinqAsync(HttpRequest request, [FromQuery] string where = null, [FromQuery] string orderBy = null, [FromQuery] string select = null, [FromQuery] string include = null, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] int? totalRecords = null) {
            return await _client.ForwardAsync<TEntity>(request, $"{ControllerPath}/linq/async");
        }

        public IEnumerable<TEntity> GetWithOData([FromQuery] string select, [FromQuery] string orderBy, [FromQuery] string filter, [FromQuery] string expand, [FromQuery] int skip, [FromQuery] int top) {
            throw new System.NotImplementedException();
        }

        public virtual string ControllerPath {
            get {
                return $"{ApiConstants.ROUTE_PREFIX}{ControllerName}";
            }
        }

        public abstract string ControllerName { get; } 
    }
}
