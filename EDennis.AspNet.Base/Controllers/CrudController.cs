using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base {
    [Route("api/[controller]")]
    [ApiController]
    public class CrudController<TContext, TEntity> : QueryController<TContext, TEntity>
        where TContext : DbContext
        where TEntity : CrudEntity {

        protected readonly string _sysUser;


        #region Overrideable Methods

        protected virtual void BeforeUpdate(TEntity existing) { }
        protected virtual void BeforeDelete(TEntity existing) { }

        public virtual void DoCreate(TEntity input) => _dbContext.Add(input);
        public virtual void DoUpdate(TEntity input, TEntity existing) => existing.Update(input);
        public virtual void DoPatch(JsonElement input, TEntity existing) => existing.Patch(input);
        public virtual void DoDelete(TEntity existing) => _dbContext.Remove(existing);

        public KeyValuesDelegate KeyValues = (p) => p.Split('~');

        #endregion

        public delegate object[] KeyValuesDelegate(string pathParameter);

        delegate object[] KeyValuesInputDelegate(TEntity input);
        KeyValuesInputDelegate KeyValuesInput;

        public virtual string KeyTemplate { get; private set; }

        public IQueryable<TEntity> Find(string pathParameter)
            => _dbContext.Set<TEntity>().Where(string.Format(KeyTemplate, KeyValues(pathParameter)));

        public IQueryable<TEntity> Find(TEntity input)
            => _dbContext.Set<TEntity>().Where(string.Format(KeyTemplate, KeyValuesInput(input)));


        private static IReadOnlyList<IProperty> Keys { get; set; }
        private static readonly Type[] _quotedTypes = {
            typeof(string), typeof(TimeSpan), typeof(TimeSpan?), typeof(DateTime), typeof(DateTime?), typeof(DateTimeOffset), typeof(DateTimeOffset?)
        };

        private void Initialize() {
            var entityType = _dbContext.Model.GetEntityTypes().FirstOrDefault(e => e.ClrType == typeof(TEntity));
            var pk = entityType.FindPrimaryKey();
            Keys = pk.Properties;
            var clauses = new List<string>();
            
            for(int i = 0; i < Keys.Count; i++) {
                var key = Keys[i];
                if (_quotedTypes.Contains(key.ClrType))
                    clauses.Add($"{key.Name} eq \"{{{i}}}\"");
                else
                    clauses.Add($"{key.Name} eq {{{i}}}");
            }
            
            KeyTemplate = string.Join(" and ", clauses);
            
            KeyValues = (p) => p.Split('~');

            KeyValuesInput = (input) => {
                var type = typeof(TEntity);
                var values = new List<object>();
                for (int i = 0; i < Keys.Count; i++) {
                    var key = Keys[i];
                    values.Add(type.GetProperty(Keys[i].Name).GetValue(input));
                }
                return values.ToArray();
            };


        }



        public CrudController(TContext context) : base(context) {
            if (KeyTemplate == null)
                Initialize();

            _sysUser = HttpContext.User.Claims
                .OrderByDescending(c => c.Type)
                .FirstOrDefault(c => c.Type == "name"
                    || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                    || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email"
                    || c.Type == "email"
                    || c.Type == "client_id")
                .Value;
        }




        [HttpGet("{key:alpha}")]
        public virtual IActionResult GetById([FromRoute] string key) {
            var qry = Find(key);
            AdjustQuery(ref qry);
            var entity = qry.FirstOrDefault();

            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }


        [HttpGet("async/{key:alpha}")]
        public virtual async Task<IActionResult> GetByIdAsync([FromRoute] string key) {
            var qry = Find(key);
            AdjustQuery(ref qry);
            var entity = (await qry.ToListAsync()).FirstOrDefault();

            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }





        [HttpPost]
        public virtual IActionResult Create([FromBody] TEntity input) {

            input.SysUser = _sysUser;
            DoCreate(input);

            try {
                _dbContext.SaveChanges();
                return Ok(input);
            } catch (DbUpdateException) {
                var key = JsonSerializer.Serialize(KeyValuesInput(input));
                if (Find(input) != null) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified key {key} already exists");
                    return Conflict(ModelState);
                } else {
                    throw;
                }
            } catch (Exception) {
                throw;
            }
        }

        [HttpPost("async")]
        public virtual async Task<IActionResult> CreateAsync([FromBody] TEntity input) {
            input.SysUser = _sysUser;
            DoCreate(input);

            try {
                await _dbContext.SaveChangesAsync();
                return Ok(input);
            } catch (DbUpdateException) {
                if (Find(input) != null) {
                    var ids = JsonSerializer.Serialize(KeyValuesInput(input));
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {ids} already exists");
                    return Conflict(ModelState);
                } else {
                    throw;
                }
            } catch (Exception) {
                throw;
            }
        }


        [HttpPut("{key:alpha}")]
        public virtual IActionResult Update([FromRoute] string key, [FromBody] TEntity input) {

            //retrieve the existing entity
            var existing = Find(key).FirstOrDefault();

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeUpdate(existing);
            DoUpdate(input, existing);
            existing.SysUser = _sysUser;

            try {
                _dbContext.SaveChanges();
                return Ok(existing);
            } catch (DbUpdateConcurrencyException ex2) {
                ModelState.AddModelError("", ex2.Message);
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (DbUpdateException ex1) {
                ModelState.AddModelError("", ex1.InnerException.Message);
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (Exception) {
                throw;
            }
        }


        [HttpPut("async/{key:alpha}")]
        public virtual async Task<IActionResult> UpdateAsync([FromRoute] string key, [FromBody] TEntity input) {

            //retrieve the existing entity
            var existing = Find(key).FirstOrDefault();

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeUpdate(existing);
            DoUpdate(input, existing);
            existing.SysUser = _sysUser;

            try {
                await _dbContext.SaveChangesAsync();
                return Ok(existing);
            } catch (DbUpdateConcurrencyException ex2) {
                ModelState.AddModelError("", ex2.Message);
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (DbUpdateException ex1) {
                ModelState.AddModelError("", ex1.InnerException.Message);
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (Exception) {
                throw;
            }
        }


        [HttpPatch("{key:alpha}")]
        public virtual IActionResult Patch([FromRoute] string key, JsonElement input) {

            if (input.ValueKind != JsonValueKind.Object)
                return new ObjectResult($"Cannot update {typeof(TEntity).Name} with {input.GetRawText().Substring(0, 200) + "..."}") { StatusCode = (int)HttpStatusCode.UnprocessableEntity };

            //retrieve the existing entity
            var existing = Find(key).FirstOrDefault();

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeUpdate(existing);
            DoPatch(input, existing);
            existing.SysUser = _sysUser; //must be after, when a patch

            try {
                _dbContext.SaveChanges();
                return Ok(existing);
            } catch (DbUpdateConcurrencyException ex2) {
                ModelState.AddModelError("", ex2.Message);
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (DbUpdateException ex1) {
                ModelState.AddModelError("", ex1.InnerException.Message);
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return Conflict(ModelState);
            }
        }


        [HttpPatch("async/{key}")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] string key, JsonElement input) {

            if (input.ValueKind != JsonValueKind.Object)
                return new ObjectResult($"Cannot update {typeof(TEntity).Name} with {input.GetRawText().Substring(0, 200) + "..."}") { StatusCode = (int)HttpStatusCode.UnprocessableEntity };

            //retrieve the existing entity
            var existing = await _dbContext.FindAsync<TEntity>(key);

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeUpdate(existing);
            DoPatch(input, existing);
            existing.SysUser = _sysUser; //must be after, when a patch

            try {
                await _dbContext.SaveChangesAsync();
                return Ok(existing);
            } catch (DbUpdateConcurrencyException ex2) {
                ModelState.AddModelError("", ex2.Message);
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (DbUpdateException ex1) {
                ModelState.AddModelError("", ex1.InnerException.Message);
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (Exception ex) {
                ModelState.AddModelError("", ex.Message);
                return Conflict(ModelState);
            }
        }

        [HttpDelete("{key:alpha}")]
        public virtual IActionResult Delete([FromRoute] string key) {

            var existing = Find(key).FirstOrDefault();

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeDelete(existing);
            DoDelete(existing);

            try {
                _dbContext.SaveChanges();
            } catch (DbUpdateConcurrencyException ex2) {
                ModelState.AddModelError("",$"Delete of { typeof(TEntity).Name }({key}) created concurrency conflict: {ex2.InnerException.Message}");
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (DbUpdateException ex1) {
                ModelState.AddModelError("", $"Could not delete { typeof(TEntity).Name }({key}): {ex1.InnerException.Message}");
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            }

            return NoContent();
        }


        [HttpDelete("async/{key:alpha}")]
        public async virtual Task<IActionResult> DeleteAsync([FromRoute] string key) {
            var existing = await _dbContext.FindAsync<TEntity>(key);

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeDelete(existing);
            DoDelete(existing);

            try {
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException ex2) {
                ModelState.AddModelError("", $"Delete of { typeof(TEntity).Name }({key}) created concurrency conflict: {ex2.InnerException.Message}");
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (DbUpdateException ex1) {
                ModelState.AddModelError("", $"Could not delete { typeof(TEntity).Name }({key}): {ex1.InnerException.Message}");
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            }

            return NoContent();
        }


        private bool NotFoundGoneLocked(TEntity entity, out IActionResult result) {

            result = null;
            if (entity != null && entity.SysStatus != SysStatus.Deleted && entity.SysStatus != SysStatus.Locked)
                return false;

            var keyValues = JsonSerializer.Serialize(KeyValuesInput(entity));

            if (entity == null) {
                ModelState.AddModelError("", $"A {entity.GetType().Name} record with Id = {keyValues} could not be found.");
                result = NotFound(ModelState);
                return true;
            } else if (entity.SysStatus == SysStatus.Deleted) {
                ModelState.AddModelError("", $"A {entity.GetType().Name} record with Id = {keyValues} was deleted.");
                result = new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Gone };
                return true;
            } else if (entity.SysStatus == SysStatus.Locked) {
                ModelState.AddModelError("", $"The {entity.GetType().Name} record with Id = {keyValues} is locked.");
                result = new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Locked };
                return true;
            } else
                return false;
        }


    }
}
