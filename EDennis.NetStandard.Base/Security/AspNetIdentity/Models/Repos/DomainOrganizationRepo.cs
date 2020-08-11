using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    public class DomainOrganizationRepo {

        public DomainIdentityDbContext _dbContext;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");

        public DomainOrganizationRepo(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }


        /// <summary>
        /// Get an instance of a DomainOrganization, based upon either:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string Name</item>
        /// </list>
        /// </summary>
        /// <param name="pathParameter">Id or Name</param>
        /// <returns>ObjectResult holding either:
        /// <list type="bullet">
        /// <item>DomainOrganization with Success status code</item>
        /// <item>null object with Not Found status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var organization = await FindAsync(pathParameter);
            if (organization == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                return new ObjectResult(organization) { StatusCode = StatusCodes.Status200OK };
            }
        }


        /// <summary>
        /// Gets a paged list of DomainOrganizations.
        /// </summary>
        /// <param name="pageNumber">Page number of recordset</param>
        /// <param name="pageSize">Page size of recordset</param>
        /// <returns>Paged list of DomainOrganization records</returns>
        public async Task<ObjectResult> GetAsync(int? pageNumber = 1, int? pageSize = 100) {

            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            var qry = _dbContext.Set<DomainOrganization>() as IQueryable<DomainOrganization>;

            qry = qry.Skip(skip)
                .Take(take)
                .AsNoTracking();

            var result = await qry.ToListAsync();

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };
        }



        /// <summary>
        /// Creates multiple organization records.
        /// Note that a JSON array is used so that the model can accomodate
        /// arbitrary extra properties that get packed as a JSON Properties 
        /// column in the AspNetOrganizations table
        /// </summary>
        /// <param name="jsonElement">A JSON array of organizations to create.</param>
        /// <param name="modelState">Object for holding errors</param>
        /// <param name="sysUser">The SysUser to associate with created records</param>
        /// <returns>Null object with Status Code</returns>
        public async Task<ObjectResult> BatchCreateAsync(JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {
            if (jsonElement.ValueKind == JsonValueKind.Array) {
                ObjectResult result = null;
                foreach (var org in jsonElement.EnumerateArray()) {
                    var localResult = await CreateAsync(org, modelState, sysUser);
                    if (result == null || localResult.StatusCode >= 400)
                        result = localResult;
                }
                return result;
            } else {
                return await CreateAsync(jsonElement, modelState, sysUser);
            }
        }



        /// <summary>
        /// Creates a single organization.
        /// Note that a JSON object is used so that the model can accomodate
        /// arbitrary extra properties that get packed as a JSON Properties 
        /// column in the AspNetOrganizations table
        /// </summary>
        /// <param name="jsonElement">A JSON object of an organization to create</param>
        /// <param name="modelState">Object for holding errors</param>
        /// <param name="sysUser">The SysUser to associate with created record</param>
        /// <returns>One of the following:
        /// <list type="bullet">
        /// <item>Created DomainOrganization object and Success status code</item>
        /// <item>Null object and Conflict status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> CreateAsync(JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {

            var inputOrg = new DomainOrganization();
            DeserializeInto(inputOrg, jsonElement, modelState, sysUser);

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };

            try {
                _dbContext.Add(inputOrg);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                modelState.AddModelError("", ex.Message);
            }


            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(inputOrg) { StatusCode = StatusCodes.Status200OK };

        }


        /// <summary>
        /// Updates a single organization.
        /// Note that a JSON object is used for two reasons:
        /// <list type="bullet">
        /// <item>The model can accomodate arbitrary extra properties that 
        /// get packed as a JSON Properties column in the AspNetOrganizations table
        /// </item>
        /// <item>Only properties provided in the JSON element are 
        /// updated.  Hence, this allows for partial updates.
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string Name</item>
        /// </list>
        /// </param>
        /// <param name="jsonElement">A JSON object of an organization to patch</param>
        /// <param name="modelState">Object for holding errors</param>
        /// <param name="sysUser">The SysUser to associate with patched record</param>
        /// <returns>One of the following:
        /// <list type="bullet">
        /// <item>Updated DomainOrganization object and Success status code</item>
        /// <item>Null object and Conflict status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> PatchAsync(string pathParameter, JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {

            var existingOrg = await FindAsync(pathParameter);

            if (existingOrg == null) {
                modelState.AddModelError("pathParameter", $"An org designated by '{pathParameter}' does not exist.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status404NotFound };
            }

            DeserializeInto(existingOrg, jsonElement, modelState, sysUser);

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };

            try {
                _dbContext.Update(existingOrg);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                modelState.AddModelError("", ex.Message);
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(existingOrg) { StatusCode = StatusCodes.Status200OK };

        }



        /// <summary>
        /// Deletes a single organization record.  Note that the record's
        /// SysStatus is updated to Deleted and saved before deleting, which
        /// ensures an audit trail of the person who deleted the record
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string Name</item>
        /// </list>
        /// </param>
        /// <param name="modelState">Object for holding errors</param>
        /// <param name="sysUser">The SysUser to associate with deleted record</param>
        /// <returns></returns>
        public async Task<ObjectResult> DeleteAsync(string pathParameter, ModelStateDictionary modelState,
            string sysUser) {

            var existingOrg = await FindAsync(pathParameter);

            if (existingOrg == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };


            //first, try to update the record with a Deleted status and the deleting user;
            //however, just ignore if there is an error.
            try {
                existingOrg.SysStatus = SysStatus.Deleted;
                existingOrg.SysUser = sysUser;
                _dbContext.Update(existingOrg);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                modelState.AddModelError("", ex.Message);
            }


            //second, actually delete the record
            try {
                _dbContext.Remove(existingOrg);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                modelState.AddModelError("", ex.Message);
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };

        }


        /// <summary>
        /// Finds DomainOrganizations by the provided path parameter
        /// </summary>
        /// <param name="pathParameter">Id, Email, or UserName</param>
        /// <returns>DomainOrganization record</returns>
        private async Task<DomainOrganization> FindAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _dbContext.FindAsync<DomainOrganization>(Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<DomainOrganization>()
                    .FirstOrDefaultAsync(a => a.Name == pathParameter);
        }


        /// <summary>
        /// Deserializes the provided JsonElement into the referenced DomainOrganization object
        /// </summary>
        /// <param name="user">Referenced DomainOrganization object (new or existing)</param>
        /// <param name="jsonElement">Parsed JSON object</param>
        /// <param name="modelState">Object to hold errors</param>
        /// <param name="sysUser">SysUser to update in user record</param>
        private void DeserializeInto(DomainOrganization org, JsonElement jsonElement, ModelStateDictionary modelState, string sysUser) {

            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Id":
                        case "id":
                            org.Id = prop.Value.GetInt32();
                            break;
                        case "Name":
                        case "name":
                            org.Name = prop.Value.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            org.SysUser = sysUser;
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            org.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            org.SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            org.SysEnd = prop.Value.GetDateTime();
                            break;
                    }
                } catch (Exception ex) {
                    modelState.AddModelError(prop.Name, $"Parsing error: {ex.Message}");
                }
            }
        }


    }

}