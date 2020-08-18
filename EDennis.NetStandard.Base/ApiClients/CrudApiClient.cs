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
    public abstract class CrudApiClient<TEntity> : QueryApiClient<TEntity>, ICrudApiClient<TEntity> where TEntity : class, ICrudEntity {

        protected CrudApiClient(IHttpClientFactory clientFactory, ITokenService tokenService, ScopedRequestMessage scopedRequestMessage)
            : base(clientFactory, tokenService, scopedRequestMessage) {
        }

        public ObjectResult<TEntity> Create([FromBody] TEntity input) {
            return _client.Post($"{ControllerPath}", input, _scopedRequestMessage);
        }

        public async Task<ObjectResult<TEntity>> CreateAsync([FromBody] TEntity input) {
            return await _client.PostAsync($"{ControllerPath}/async", input, _scopedRequestMessage);
        }

        public StatusCodeResult Delete([FromRoute] string key) {
            return _client.Delete<TEntity>($"{ControllerPath}/{key}", _scopedRequestMessage);
        }

        public async Task<StatusCodeResult> DeleteAsync([FromRoute] string key) {
            return await _client.DeleteAsync<TEntity>($"{ControllerPath}/async/{key}", _scopedRequestMessage);
        }

        public IQueryable<TEntity> Find(string pathParameter) {
            throw new System.NotImplementedException();
        }


        public ObjectResult<TEntity> GetById([FromRoute] string key) {
            return _client.Get<TEntity>($"{ControllerPath}/{key}", _scopedRequestMessage);
        }


        public async Task<ObjectResult<TEntity>> GetByIdAsync([FromRoute] string key) {
            return await _client.GetAsync<TEntity>($"{ControllerPath}/async/{key}", _scopedRequestMessage);
        }


        public ObjectResult<TEntity> Patch([FromRoute] string key, [FromBody] JsonElement input) {
            return _client.Patch<JsonElement, TEntity>($"{ControllerPath}/{key}", input, _scopedRequestMessage);
        }


        public async Task<ObjectResult<TEntity>> PatchAsync([FromRoute] string key, [FromBody] JsonElement input) {
            return await _client.PatchAsync<JsonElement, TEntity>($"{ControllerPath}/async/{key}", input, _scopedRequestMessage);
        }


        public ObjectResult<TEntity> Update([FromRoute] string key, [FromBody] TEntity input) {
            return _client.Put($"{ControllerPath}/{key}", input, _scopedRequestMessage);
        }

        public async Task<ObjectResult<TEntity>> UpdateAsync([FromRoute] string key, [FromBody] TEntity input) {
            return await _client.PutAsync($"{ControllerPath}/async/{key}", input, _scopedRequestMessage);
        }
    }
}
