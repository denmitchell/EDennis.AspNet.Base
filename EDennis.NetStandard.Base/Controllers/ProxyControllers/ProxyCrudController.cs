using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Base proxy ("external") controller for communicating with an
    /// internal controller via secure HttpClient.
    /// Note: in the ProxyClients configuration section, each proxy client
    /// should have a key equal to the name of the corresponding proxy controller
    /// minus the word 'controller'
    /// Note: setup the DependencyInjection to include:
    /// <list type="bullet">
    /// <item>AddSecureTokenService<TTokenService>(Configuration,"Security:ClientCredentials")</item>
    /// <item>AddProxyClients(Configuration,"ProxyClients");</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [Route(ApiConstants.ROUTE_PREFIX + "[controller]")]
    [ApiController]
    public abstract class ProxyCrudController<TEntity> : ProxyQueryController<TEntity>, ICrudController<TEntity>
        where TEntity : class, ICrudEntity {


        public ProxyCrudController(IHttpClientFactory clientFactory, ITokenService tokenService) :
            base(clientFactory,tokenService) { }


        [HttpPost]
        public IActionResult Create([FromBody] TEntity input) {
            return _client.Forward(HttpContext.Request, $"{ControllerPath}", input);
        }

        [HttpPost("async")]
        public async Task<IActionResult> CreateAsync([FromBody] TEntity input) {
            return await _client.ForwardAsync(HttpContext.Request, $"{ControllerPath}/async", input);
        }

        [HttpDelete("{**key}")]
        public IActionResult Delete([FromRoute] string key) {
            return _client.Forward<StatusCodeResult>(HttpContext.Request, $"{ControllerPath}/{key}");
        }

        [HttpDelete("async/{**key}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] string key) {
            return await _client.ForwardAsync<StatusCodeResult>(HttpContext.Request, $"{ControllerPath}/{key}");
        }

        [NonAction]
        public IQueryable<TEntity> Find(string pathParameter) {
            throw new System.NotImplementedException();
        }


        [HttpGet("{**key}")]
        public IActionResult GetById([FromRoute] string key) {
            return _client.Forward<TEntity>(HttpContext.Request, $"{ControllerPath}/{key}");
        }


        [HttpGet("async/{**key}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] string key) {
            return await _client.ForwardAsync<TEntity>(HttpContext.Request, $"{ControllerPath}/{key}");
        }


        [HttpPatch("{**key}")]
        public IActionResult Patch([FromRoute] string key, [FromBody] JsonElement input) {
            return _client.Forward<JsonElement,TEntity>(HttpContext.Request, $"{ControllerPath}/{key}", input);
        }


        [HttpPatch("async/{**key}")]
        public async Task<IActionResult> PatchAsync([FromRoute] string key, [FromBody] JsonElement input) {
            return await _client.ForwardAsync<JsonElement, TEntity>(HttpContext.Request, $"{ControllerPath}/{key}", input);
        }


        [HttpPut("{**key}")]
        public IActionResult Update([FromRoute] string key, [FromBody] TEntity input) {
            return _client.Forward(HttpContext.Request, $"{ControllerPath}/{key}", input);
        }

        [HttpPut("async/{**key}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] string key, [FromBody] TEntity input) {
            return await _client.ForwardAsync(HttpContext.Request, $"{ControllerPath}/{key}", input);
        }
    }
}
