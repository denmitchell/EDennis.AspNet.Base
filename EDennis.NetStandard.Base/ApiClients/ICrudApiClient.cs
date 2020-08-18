using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public interface ICrudApiClient<TEntity> where TEntity : class, ICrudEntity {
        ObjectResult<TEntity> Create([FromBody] TEntity input);
        Task<ObjectResult<TEntity>> CreateAsync([FromBody] TEntity input);
        StatusCodeResult Delete([FromRoute] string key);
        Task<StatusCodeResult> DeleteAsync([FromRoute] string key);
        IQueryable<TEntity> Find(string pathParameter);
        ObjectResult<TEntity> GetById([FromRoute] string key);
        Task<ObjectResult<TEntity>> GetByIdAsync([FromRoute] string key);
        ObjectResult<TEntity> Patch([FromRoute] string key, [FromBody] JsonElement input);
        Task<ObjectResult<TEntity>> PatchAsync([FromRoute] string key, [FromBody] JsonElement input);
        ObjectResult<TEntity> Update([FromRoute] string key, [FromBody] TEntity input);
        Task<ObjectResult<TEntity>> UpdateAsync([FromRoute] string key, [FromBody] TEntity input);
    }
}