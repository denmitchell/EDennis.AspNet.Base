using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class CrudApiClient<TEntity> : QueryApiClient<TEntity>
        where TEntity : class, ICrudEntity {


        public CrudApiClient(IHttpClientFactory clientFactory, ITokenService tokenService) :
            base(clientFactory,tokenService) { }


        public IActionResult Create([FromBody] TEntity input) {
            return _client.Post($"{ControllerPath}",input);
        }

        public async Task<IActionResult> CreateAsync([FromBody] TEntity input) {
            return await _client.PostAsync($"{ControllerPath}/async", input);
        }

        public IActionResult Delete([FromRoute] string key) {
            return _client.Delete<TEntity>($"{ControllerPath}/{key}");
        }

        public async Task<IActionResult> DeleteAsync([FromRoute] string key) {
            return await _client.DeleteAsync<TEntity>($"{ControllerPath}/async/{key}");
        }

        public IQueryable<TEntity> Find(string pathParameter) {
            throw new System.NotImplementedException();
        }


        public IActionResult GetById([FromRoute] string key) {
            return _client.Get<TEntity>($"{ControllerPath}/{key}");
        }


        public async Task<IActionResult> GetByIdAsync([FromRoute] string key) {
            return await _client.GetAsync<TEntity>($"{ControllerPath}/async/{key}");
        }


        public IActionResult Patch([FromRoute] string key, [FromBody] JsonElement input) {
            return _client.Patch($"{ControllerPath}/{key}", input);
        }


        public async Task<IActionResult> PatchAsync([FromRoute] string key, [FromBody] JsonElement input) {
            return await _client.PatchAsync($"{ControllerPath}/async/{key}", input);
        }


        public IActionResult Update([FromRoute] string key, [FromBody] TEntity input) {
            return _client.Put($"{ControllerPath}/{key}", input);
        }

        public async Task<IActionResult> UpdateAsync([FromRoute] string key, [FromBody] TEntity input) {
            return await _client.PutAsync($"{ControllerPath}/async/{key}", input);
        }
    }
}
