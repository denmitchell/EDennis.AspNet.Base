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

    public class DomainOrganizationRepo {

        public DomainIdentityDbContext _dbContext;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");

        public DomainOrganizationRepo(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }


        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var organization = await FindAsync(pathParameter);
            if (organization == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                return new ObjectResult(organization) { StatusCode = StatusCodes.Status200OK };
            }
        }





        public async Task<ObjectResult> GetAsync(int? pageNumber = 1,
            int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            var qry = _dbContext.Organizations as IQueryable<DomainOrganization>;

            qry = qry.Skip(skip)
                .Take(take)
                .AsNoTracking();

            var result = await qry.ToListAsync();

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };

        }



        private async Task<DomainOrganization> FindAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _dbContext.FindAsync<DomainOrganization>(Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<DomainOrganization>()
                    .FirstOrDefaultAsync(a => a.Name == pathParameter);
        }


        /// <summary>
        /// Note: the create method accepts a JsonElement to permit the addition of
        /// any number of arbitrary properties, which get packed into a "Properties"
        /// property as a JSON object string.  The DomainOrganizationJsonConverter
        /// unpacks the extra properties and promotes them to top-level JSON
        /// properties during serialization.
        /// </summary>
        /// <param name="jsonElement"></param>
        /// <param name="modelState"></param>
        /// <param name="sysUser"></param>
        /// <returns></returns>
        public async Task<ObjectResult> CreateAsync(JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {

            var newOrganization = new DomainOrganization();
            newOrganization.DeserializeInto(jsonElement, modelState);

            newOrganization.Id = CombGuid.Create();
            newOrganization.SysUser = sysUser;
            newOrganization.SysStatus = SysStatus.Normal;


            var existingOrganization = _dbContext.Set<DomainOrganization>().FirstOrDefault(r => r.Name == newOrganization.Name);

            if (existingOrganization != null) {
                modelState.AddModelError("Name", $"An organization with Name ='{newOrganization.Name}' already exists.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }


            try {
                await _dbContext.AddAsync(newOrganization);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot add organization '{newOrganization.Name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(newOrganization) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> PatchAsync(string name, JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {


            var existingOrganization = _dbContext.Set<DomainOrganization>().FirstOrDefault(r => r.Name == name);

            if (existingOrganization == null) {
                modelState.AddModelError("Name", $"An organization with Name ='{name}' does not exist.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status404NotFound };
            }

            existingOrganization.DeserializeInto(jsonElement, modelState);
            existingOrganization.SysUser = sysUser;


            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status400BadRequest };


            try {
                _dbContext.Update(existingOrganization);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot update organization '{name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(existingOrganization) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> DeleteAsync(string name, ModelStateDictionary modelState,
            string sysUser) {

            var existingOrganization = _dbContext.Set<DomainOrganization>().FirstOrDefault(r => r.Name == name);

            if (existingOrganization == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };

            //first, try to update the record with a Deleted status and the deleting user;
            //however, just ignore if there is an error.
            try {
                existingOrganization.SysStatus = SysStatus.Deleted;
                existingOrganization.SysUser = sysUser;
                _dbContext.Update(existingOrganization);
                await _dbContext.SaveChangesAsync();
            } catch (Exception) { }


            //second, actually delete the record
            try {
                _dbContext.Remove(existingOrganization);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot delete organization '{name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };

        }

    }

}