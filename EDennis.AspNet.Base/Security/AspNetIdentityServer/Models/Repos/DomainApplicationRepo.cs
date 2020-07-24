using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {

    public class DomainApplicationRepo {

        public DomainIdentityDbContext _dbContext;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");

        public DomainApplicationRepo(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }


        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var application = await FindAsync(pathParameter);
            if (application == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                return new ObjectResult(application) { StatusCode = StatusCodes.Status200OK };
            }
        }





        public async Task<ObjectResult> GetAsync(int? pageNumber = 1, 
            int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            var qry = _dbContext.Applications as IQueryable<DomainApplication>;

            qry = qry.Skip(skip)
                .Take(take)
                .AsNoTracking();
                
            var result = await qry.ToListAsync();

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };

        }



        private async Task<DomainApplication> FindAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _dbContext.FindAsync<DomainApplication>(Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<DomainApplication>()
                    .FirstOrDefaultAsync(a=>a.Name == pathParameter);
        }


        /// <summary>
        /// Note: the create method accepts a JsonElement to permit the addition of
        /// any number of arbitrary properties, which get packed into a "Properties"
        /// property as a JSON object string.  The DomainApplicationJsonConverter
        /// unpacks the extra properties and promotes them to top-level JSON
        /// properties during serialization.
        /// </summary>
        /// <param name="jsonElement"></param>
        /// <param name="modelState"></param>
        /// <param name="sysUser"></param>
        /// <returns></returns>
        public async Task<ObjectResult> CreateAsync(JsonElement jsonElement, 
            ModelStateDictionary modelState, string sysUser) {

            var newApplication = new DomainApplication();
            newApplication.DeserializeInto(jsonElement, modelState);

            newApplication.Id = CombGuid.Create();
            newApplication.SysUser = sysUser;
            newApplication.SysStatus = SysStatus.Normal;


            var existingApplication = _dbContext.Set<DomainApplication>().FirstOrDefault(r => r.Name == newApplication.Name);

            if (existingApplication != null) {
                modelState.AddModelError("Name", $"An application with Name ='{newApplication.Name}' already exists.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }


            try {
                await _dbContext.AddAsync(newApplication);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot add application '{newApplication.Name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(newApplication) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> PatchAsync(string name, JsonElement jsonElement, 
            ModelStateDictionary modelState, string sysUser) {


            var existingApplication = _dbContext.Set<DomainApplication>().FirstOrDefault(r => r.Name == name);

            if (existingApplication == null) {
                modelState.AddModelError("Name", $"An application with Name ='{name}' does not exist.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status404NotFound };
            }

            existingApplication.DeserializeInto(jsonElement, modelState);
            existingApplication.SysUser = sysUser;


            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status400BadRequest };


            try {
                _dbContext.Update(existingApplication);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot update application '{name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(existingApplication) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> DeleteAsync(string name, ModelStateDictionary modelState, 
            string sysUser) {

            var existingApplication = _dbContext.Set<DomainApplication>().FirstOrDefault(r => r.Name == name);

            if (existingApplication == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };

            //first, try to update the record with a Deleted status and the deleting user;
            //however, just ignore if there is an error.
            try {
                existingApplication.SysStatus = SysStatus.Deleted;
                existingApplication.SysUser = sysUser;
                _dbContext.Update(existingApplication);
                await _dbContext.SaveChangesAsync();
            } catch (Exception) { }


            //second, actually delete the record
            try {
                _dbContext.Remove(existingApplication);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot delete application '{name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };

        }

    }

}