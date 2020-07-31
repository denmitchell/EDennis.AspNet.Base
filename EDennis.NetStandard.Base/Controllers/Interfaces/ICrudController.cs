using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public interface ICrudController<TEntity> where TEntity : class, ICrudEntity {
        IActionResult Create([FromBody] TEntity input);
        Task<IActionResult> CreateAsync([FromBody] TEntity input);
        IActionResult Delete([FromRoute] string key);
        Task<IActionResult> DeleteAsync([FromRoute] string key);
        IQueryable<TEntity> Find(string pathParameter);
        IActionResult GetById([FromRoute] string key);
        Task<IActionResult> GetByIdAsync([FromRoute] string key);
        IActionResult Patch([FromRoute] string key, [FromBody] JsonElement input);
        Task<IActionResult> PatchAsync([FromRoute] string key, [FromBody] JsonElement input);
        IActionResult Update([FromRoute] string key, [FromBody] TEntity input);
        Task<IActionResult> UpdateAsync([FromRoute] string key, [FromBody] TEntity input);
    }
}