using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Blazor Api Client for communicating with an
    /// internal controller via secure HttpClient.
    /// Note: Uses integrated Identity Server and Microsoft-generated
    /// DI for HttpClientFactory/HttpClient in the Blazor Client Program class.  
    /// Just extend this class and AddScoped<MyExtendedBlazorQueryApiClient>()
    /// </summary>
    public abstract class BlazorCrudApiClient<TEntity> : BlazorQueryApiClient<TEntity>, ICrudApiClient<TEntity> where TEntity : class, ICrudEntity {

        protected BlazorCrudApiClient(HttpClient client) : base(client) {
        }

        public ObjectResult<TEntity> Create([FromBody] TEntity input) {
            return HttpClient.Post($"{ControllerPath}", input, _scopedRequestMessage);
        }

        public async Task<ObjectResult<TEntity>> CreateAsync([FromBody] TEntity input) {
            return await HttpClient.PostAsync($"{ControllerPath}/async", input, _scopedRequestMessage);
        }

        public StatusCodeResult Delete([FromRoute] string key) {
            return HttpClient.Delete<TEntity>($"{ControllerPath}/{key}", _scopedRequestMessage);
        }

        public async Task<StatusCodeResult> DeleteAsync([FromRoute] string key) {
            return await HttpClient.DeleteAsync<TEntity>($"{ControllerPath}/async/{key}", _scopedRequestMessage);
        }

        public IQueryable<TEntity> Find(string pathParameter) {
            throw new System.NotImplementedException();
        }


        public ObjectResult<TEntity> GetById([FromRoute] string key) {
            return HttpClient.Get<TEntity>($"{ControllerPath}/{key}", _scopedRequestMessage);
        }


        public async Task<ObjectResult<TEntity>> GetByIdAsync([FromRoute] string key) {
            return await HttpClient.GetAsync<TEntity>($"{ControllerPath}/async/{key}", _scopedRequestMessage);
        }


        public ObjectResult<TEntity> Patch([FromRoute] string key, [FromBody] JsonElement input) {
            return HttpClient.Patch<JsonElement, TEntity>($"{ControllerPath}/{key}", input, _scopedRequestMessage);
        }


        public async Task<ObjectResult<TEntity>> PatchAsync([FromRoute] string key, [FromBody] JsonElement input) {
            return await HttpClient.PatchAsync<JsonElement, TEntity>($"{ControllerPath}/async/{key}", input, _scopedRequestMessage);
        }


        public ObjectResult<TEntity> Update([FromRoute] string key, [FromBody] TEntity input) {
            return HttpClient.Put($"{ControllerPath}/{key}", input, _scopedRequestMessage);
        }

        public async Task<ObjectResult<TEntity>> UpdateAsync([FromRoute] string key, [FromBody] TEntity input) {
            return await HttpClient.PutAsync($"{ControllerPath}/async/{key}", input, _scopedRequestMessage);
        }
    }
}
