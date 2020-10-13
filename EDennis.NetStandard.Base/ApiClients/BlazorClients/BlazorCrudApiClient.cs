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
            var result = new ObjectResult<TEntity>(default);
            try {
                result = HttpClient.Post($"{ControllerPath}", input);
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }

        public async Task<ObjectResult<TEntity>> CreateAsync([FromBody] TEntity input) {
            var result = new ObjectResult<TEntity>(default);
            try {
                return await HttpClient.PostAsync($"{ControllerPath}/async", input);
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }

        public StatusCodeResult Delete([FromRoute] string key) {
            var result = new StatusCodeResult(400);
            try {
                result = HttpClient.Delete<TEntity>($"{ControllerPath}/{key}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }

        public async Task<StatusCodeResult> DeleteAsync([FromRoute] string key) {
            var result = new StatusCodeResult(400);
            try {
                result = await HttpClient.DeleteAsync<TEntity>($"{ControllerPath}/async/{key}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }

        public IQueryable<TEntity> Find(string pathParameter) {
            throw new System.NotImplementedException();
        }


        public ObjectResult<TEntity> GetById([FromRoute] string key) {
            var result = new ObjectResult<TEntity>(default);
            try {
                result = HttpClient.Get<TEntity>($"{ControllerPath}/{key}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }


        public async Task<ObjectResult<TEntity>> GetByIdAsync([FromRoute] string key) {
            var result = new ObjectResult<TEntity>(default);
            try {
                result = await HttpClient.GetAsync<TEntity>($"{ControllerPath}/async/{key}");
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }


        public ObjectResult<TEntity> Patch([FromRoute] string key, [FromBody] JsonElement input) {
            var result = new ObjectResult<TEntity>(default);
            try {
                result = HttpClient.Patch<JsonElement, TEntity>($"{ControllerPath}/{key}", input);
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }


        public async Task<ObjectResult<TEntity>> PatchAsync([FromRoute] string key, [FromBody] JsonElement input) {
            var result = new ObjectResult<TEntity>(default);
            try {
                result = await HttpClient.PatchAsync<JsonElement, TEntity>($"{ControllerPath}/async/{key}", input);
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }


        public ObjectResult<TEntity> Update([FromRoute] string key, [FromBody] TEntity input) {
            var result = new ObjectResult<TEntity>(default);
            try {
                result = HttpClient.Put($"{ControllerPath}/{key}", input);
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }

        public async Task<ObjectResult<TEntity>> UpdateAsync([FromRoute] string key, [FromBody] TEntity input) {
            var result = new ObjectResult<TEntity>(default);
            try {
                result = await HttpClient.PutAsync($"{ControllerPath}/async/{key}", input);
            } catch (AccessTokenNotAvailableException exception) {
                exception.Redirect();
            }
            return result;
        }
    }
}
