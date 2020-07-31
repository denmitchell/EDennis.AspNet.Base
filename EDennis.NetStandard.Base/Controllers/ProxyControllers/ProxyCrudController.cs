using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.Controllers.ProxyControllers {
    public class ProxyCrudController<TEntity> : ICrudController<TEntity>
        where TEntity : class, ICrudEntity {

        public IActionResult Create([FromBody] TEntity input) {
            throw new System.NotImplementedException();
        }

        public Task<IActionResult> CreateAsync([FromBody] TEntity input) {
            throw new System.NotImplementedException();
        }

        public IActionResult Delete([FromRoute] string key) {
            throw new System.NotImplementedException();
        }

        public Task<IActionResult> DeleteAsync([FromRoute] string key) {
            throw new System.NotImplementedException();
        }

        public IQueryable<TEntity> Find(string pathParameter) {
            throw new System.NotImplementedException();
        }

        public IActionResult GetById([FromRoute] string key) {
            throw new System.NotImplementedException();
        }

        public Task<IActionResult> GetByIdAsync([FromRoute] string key) {
            throw new System.NotImplementedException();
        }

        public IActionResult Patch([FromRoute] string key, [FromBody] JsonElement input) {
            throw new System.NotImplementedException();
        }

        public Task<IActionResult> PatchAsync([FromRoute] string key, [FromBody] JsonElement input) {
            throw new System.NotImplementedException();
        }

        public IActionResult Update([FromRoute] string key, [FromBody] TEntity input) {
            throw new System.NotImplementedException();
        }

        public Task<IActionResult> UpdateAsync([FromRoute] string key, [FromBody] TEntity input) {
            throw new System.NotImplementedException();
        }
    }
}
