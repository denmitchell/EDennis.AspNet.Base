using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EDennis.AspNet.Base {
    [Route("api/[controller]")]
    [ApiController]
    public class CrudController<TContext, TEntity> : QueryController<TContext,TEntity>
        where TContext : DbContext
        where TEntity: CrudEntity {

        protected readonly string _sysUser;


        #region Overrideable Methods

        protected virtual void BeforeUpdate(TEntity existing) { }
        protected virtual void BeforeDelete(TEntity existing) { }

        public virtual void DoCreate(TEntity input) => _dbContext.Add(input);
        public virtual void DoUpdate(TEntity input, TEntity existing) => existing.Update(input);
        public virtual void DoPatch(JsonElement input, TEntity existing) => existing.Patch(input);
        public virtual void DoDelete(TEntity existing) => _dbContext.Remove(existing);

        #endregion


        public CrudController(DbContextProvider<TContext> provider) : base(provider) {
            _sysUser = HttpContext.User.Claims
                .OrderByDescending(c => c.Type)
                .FirstOrDefault(c => c.Type == "name"
                    || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
                    || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email"
                    || c.Type == "email"
                    || c.Type == "client_id")
                .Value;
        }




        [HttpGet("{id:int}")]
        public virtual IActionResult GetById([FromRoute] int id) {
            var qry = _dbContext.Set<TEntity>().Where(x => x.Id == id);
            AdjustQuery(ref qry);
            var entity = qry.FirstOrDefault();

            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }


        [HttpGet("async/{id:int}")]
        public virtual async Task<IActionResult> GetByIdAsync([FromRoute] int id) {
            var qry = _dbContext.Set<TEntity>().Where(x => x.Id == id);
            AdjustQuery(ref qry);
            var entity = (await qry.ToListAsync()).FirstOrDefault();

            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }



        [HttpGet("{sysId:guid}")]
        public virtual IActionResult GetById([FromRoute] Guid sysId) {
            var qry = _dbContext.Set<TEntity>().Where(x => x.SysId == sysId);
            AdjustQuery(ref qry);
            var entity = qry.FirstOrDefault();

            if (entity == null)
                return NotFound();
            else
                return Ok(entity);
        }


        [HttpGet("async/{sysId:guid}")]
        public virtual async Task<IActionResult> GetByIdAsync([FromRoute] Guid sysId) {
            var qry = _dbContext.Set<TEntity>().Where(x => x.SysId == sysId);
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
                if (_dbContext.Find<TEntity>(input.Id) != null) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {input.Id} already exists");
                    return Conflict(ModelState);
                } else {
                    throw;
                }
            } catch (Exception) {
                throw;
            }
            //return CreatedAtAction("GetById", new { id = entity.Id }, entity);
        }

        [HttpPost("async")]
        public virtual async Task<IActionResult> CreateAsync([FromBody] TEntity input) {
            input.SysUser = _sysUser;
            DoCreate(input);

            try {
                await _dbContext.SaveChangesAsync();
                return Ok(input);
            } catch (DbUpdateException) {
                if (await _dbContext.FindAsync<TEntity>(input.Id) != null) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {input.Id} already exists");
                    return Conflict(ModelState);
                } else {
                    throw;
                }
            } catch (Exception) {
                throw;
            }
            //return CreatedAtAction("GetById", new { id = entity.Id }, entity);
        }


        [HttpPut("{id}")]
        public virtual IActionResult Update([FromBody] TEntity input, [FromRoute] int id) {

            if (input.Id != id) {
                ModelState.AddModelError("", $"The path parameter id ({id}) does not match the provided object's id ({input.Id})");
                return BadRequest(ModelState);
            }

            //retrieve the existing entity
            var existing = _dbContext.Find<TEntity>(input.Id);

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeUpdate(existing);
            DoUpdate(input, existing);
            existing.SysUser = _sysUser;

            try {
                _dbContext.SaveChanges();
                return Ok(existing);
            } catch (DbUpdateConcurrencyException ex) {
                if (_dbContext.Find<TEntity>(input.Id) != null) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {input.Id} cannot be found");
                    return NotFound(ModelState);
                } else {
                    ModelState.AddModelError("", ex.Message);
                    return Conflict(ModelState);
                }
            } catch (Exception) {
                throw;
            }
            //return NoContent();
        }


        [HttpPut("async/{id}")]
        public virtual async Task<IActionResult> UpdateAsync([FromBody] TEntity input, [FromRoute] int id) {

            if (input.Id != id) {
                ModelState.AddModelError("", $"The path parameter id ({id}) does not match the provided object's id ({input.Id})");
                return BadRequest(ModelState);
            }

            //retrieve the existing entity
            var existing = await _dbContext.FindAsync<TEntity>(input.Id);

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeUpdate(existing);
            DoUpdate(input, existing);
            existing.SysUser = _sysUser;

            try {
                await _dbContext.SaveChangesAsync();
                return Ok(existing);
            } catch (DbUpdateConcurrencyException ex) {
                if (_dbContext.Find<TEntity>(input.Id) != null) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {input.Id} cannot be found");
                    return NotFound(ModelState);
                } else {
                    ModelState.AddModelError("", ex.Message);
                    return Conflict(ModelState);
                }
            } catch (Exception) {
                throw;
            }
            //return NoContent();
        }


        [HttpPatch("{id}")]
        public virtual IActionResult Patch(JsonElement input, [FromRoute] int id) {

            if (input.ValueKind != JsonValueKind.Object)
                return new ObjectResult($"Cannot update {typeof(TEntity).Name} with {input.GetRawText().Substring(0, 200) + "..."}") { StatusCode = (int)HttpStatusCode.UnprocessableEntity };

            //retrieve the existing entity
            var existing = _dbContext.Find<TEntity>(id);

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeUpdate(existing);
            DoPatch(input, existing);
            existing.SysUser = _sysUser; //must be after, when a patch

            try {
                _dbContext.SaveChanges();
                return Ok(existing);
            } catch (DbUpdateConcurrencyException ex) {
                if (_dbContext.Find<TEntity>(id) != null) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {id} cannot be found");
                    return NotFound(ModelState);
                } else {
                    ModelState.AddModelError("", ex.Message);
                    return Conflict(ModelState);
                }
            } catch (Exception) {
                throw;
            }
            //return NoContent();
        }


        [HttpPatch("async/{id}")]
        public virtual async Task<IActionResult> PatchAsync(JsonElement input, [FromRoute] int id) {

            if (input.ValueKind != JsonValueKind.Object)
                return new ObjectResult($"Cannot update {typeof(TEntity).Name} with {input.GetRawText().Substring(0, 200) + "..."}") { StatusCode = (int)HttpStatusCode.UnprocessableEntity };

            //retrieve the existing entity
            var existing = await _dbContext.FindAsync<TEntity>(id);

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeUpdate(existing);
            DoPatch(input, existing);
            existing.SysUser = _sysUser; //must be after, when a patch

            try {
                await _dbContext.SaveChangesAsync();
                return Ok(existing);
            } catch (DbUpdateConcurrencyException ex) {
                if (_dbContext.Find<TEntity>(id) != null) {
                    ModelState.AddModelError("", $"A {typeof(TEntity).Name} instance with the specified id {id} cannot be found");
                    return NotFound(ModelState);
                } else {
                    ModelState.AddModelError("", ex.Message);
                    return Conflict(ModelState);
                }
            } catch (Exception) {
                throw;
            }
            //return NoContent();
        }

        [HttpDelete("{id}")]
        public virtual IActionResult Delete([FromRoute] int id) {

            var existing = _dbContext.Find<TEntity>(id);

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeDelete(existing);
            DoDelete(existing);

            try {
                _dbContext.SaveChanges();
            } catch (DbUpdateConcurrencyException ex2) {
                ModelState.AddModelError("",$"Delete of { typeof(TEntity).Name }({id}) created concurrency conflict: {ex2.InnerException.Message}");
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (DbUpdateException ex1) {
                ModelState.AddModelError("", $"Could not delete { typeof(TEntity).Name }({id}): {ex1.InnerException.Message}");
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            }

            return NoContent();
        }


        [HttpDelete("async/{id}")]
        public async virtual Task<IActionResult> DeleteAsync([FromRoute] int id) {
            var existing = await _dbContext.FindAsync<TEntity>(id);

            //check NotFound, Gone (deleted), Locked
            if (NotFoundGoneLocked(existing, out IActionResult result))
                return result;

            BeforeDelete(existing);
            DoDelete(existing);

            try {
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException ex2) {
                ModelState.AddModelError("", $"Delete of { typeof(TEntity).Name }({id}) created concurrency conflict: {ex2.InnerException.Message}");
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            } catch (DbUpdateException ex1) {
                ModelState.AddModelError("", $"Could not delete { typeof(TEntity).Name }({id}): {ex1.InnerException.Message}");
                return new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Conflict };
            }

            return NoContent();
        }


        private bool NotFoundGoneLocked(TEntity entity, out IActionResult result) {

            result = null;

            if (entity == null) {
                ModelState.AddModelError("", $"A {entity.GetType().Name} record with Id = {entity.Id} could not be found.");
                result = NotFound(ModelState);
                return true;
            } else if (entity.SysStatus == SysStatus.Deleted) {
                ModelState.AddModelError("", $"A {entity.GetType().Name} record with Id = {entity.Id} was deleted.");
                result = new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Gone };
                return true;
            } else if (entity.SysStatus == SysStatus.Locked) {
                ModelState.AddModelError("", $"The {entity.GetType().Name} record with Id = {entity.Id} is locked.");
                result = new ObjectResult(ModelState) { StatusCode = (int)HttpStatusCode.Locked };
                return true;
            } else
                return false;
        }


    }
}
