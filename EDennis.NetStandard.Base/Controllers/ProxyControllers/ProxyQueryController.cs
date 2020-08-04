using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Base proxy ("external") controller for communicating with an
    /// internal controller via secure HttpClient.
    /// Note: setup the DependencyInjection to include:
    /// <list type="bullet">
    /// <item>AddHttpClient("{name of the controller class}", config=> { config.BaseAddress = ... ;});</item>
    /// <item>AddSingleton<ITokenService,ClientCredentialsTokenService>(); //or use MockTokenService to bypass token generation</item>
    /// </list>
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ProxyQueryController<TEntity> : ControllerBase, IQueryController<TEntity>
        where TEntity : class {

        protected readonly HttpClient _client;


        public ProxyQueryController(IHttpClientFactory clientFactory, ITokenService tokenService) {
            _client = clientFactory.CreateClient(GetType().Name);
            tokenService.AssignTokenAsync(_client).Wait();
        }

        [NonAction]
        public virtual void AdjustQuery(ref IQueryable<TEntity> query) { }



        [HttpGet("devextreme")]
        public IActionResult GetWithDevExtreme([FromQuery] string select, [FromQuery] string sort, [FromQuery] string filter, [FromQuery] int skip, [FromQuery] int take, [FromQuery] string totalSummary, [FromQuery] string group, [FromQuery] string groupSummary) {
            return _client.Forward<DeserializableLoadResult<TEntity>>(HttpContext.Request, $"{ControllerName}/devextreme");
        }


        [HttpGet("devextreme/async")]
        public async Task<IActionResult> GetWithDevExtremeAsync([FromQuery] string select, [FromQuery] string sort, [FromQuery] string filter, [FromQuery] int skip, [FromQuery] int take, [FromQuery] string totalSummary, [FromQuery] string group, [FromQuery] string groupSummary) {
            return await _client.ForwardAsync<DeserializableLoadResult<TEntity>>(HttpContext.Request, $"{ControllerName}/devextreme/async");
        }


        [HttpGet("linq")]
        public IActionResult GetWithDynamicLinq([FromQuery] string where = null, [FromQuery] string orderBy = null, [FromQuery] string select = null, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] int? totalRecords = null) {
            return _client.Forward<TEntity>(HttpContext.Request, $"{ControllerName}/linq");
        }


        [HttpGet("linq/async")]
        public async Task<IActionResult> GetWithDynamicLinqAsync([FromQuery] string where = null, [FromQuery] string orderBy = null, [FromQuery] string select = null, [FromQuery] int? skip = null, [FromQuery] int? take = null, [FromQuery] int? totalRecords = null) {
            return await _client.ForwardAsync<TEntity>(HttpContext.Request, $"{ControllerName}/linq/async");
        }

        public IEnumerable<TEntity> GetWithOData([FromQuery] string select, [FromQuery] string orderBy, [FromQuery] string filter, [FromQuery] string expand, [FromQuery] int skip, [FromQuery] int top) {
            throw new System.NotImplementedException();
        }

        protected string ControllerPath {
            get {
                return $"api/{ControllerName}";
            }
        }

        protected string ControllerName { 
            get {
                return ControllerContext.ActionDescriptor.ControllerName;
            } 
        }
    }
}
