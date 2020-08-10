using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace EDennis.NetStandard.Base {
    public class DomainRoleRepo {

        public DomainIdentityDbContext _dbContext;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");
        public static Regex lpPattern = new Regex("(?:\\(\\s*loginProvider\\s*=\\s*'?)([^,']+)(?:'?\\s*,\\s*providerKey\\s*=\\s*'?)([^)']+)(?:'?\\s*\\))");


        public DomainRoleRepo(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }



        /// <summary>
        /// Get an instance of a DomainRole, based upon the path parameter
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string Name</item>
        /// </list>
        /// <returns>ObjectResult holding either:
        /// <list type="bullet">
        /// <item>DomainRole with Success status code</item>
        /// <item>null object with Not Found status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var role = await FindAsync(pathParameter);
            if (role == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                return new ObjectResult(role) { StatusCode = StatusCodes.Status200OK };
            }
        }

        /// <summary>
        /// Gets a paged list of DomainRoles optionally by application name.
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="pageNumber">Page number of recordset</param>
        /// <param name="pageSize">Page size of recordset</param>
        /// <returns>Paged list of DomainRole records</returns>
        public async Task<ObjectResult> GetAsync(string appName = null,
            int? pageNumber = 1, int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            #region FormattableString sql
            FormattableString sql;

            if (appName != null)
                sql = $@"
select r.*
	from AspNetRoles r
    inner join AspNetApplications a
        on r.ApplicationId = a.Id
	where a.Name = {appName}
    offset {skip} rows
    fetch next {take} rows only
";
            else
                sql = $@"
select r.*
	from AspNetRoles r
    offset {skip} rows
    fetch next {take} rows only
";
            #endregion
            var result = await _dbContext.Set<DomainRole>()
                            .FromSqlInterpolated(sql)
                            .AsNoTracking()
                            .ToListAsync();

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };
        }



        /// <summary>
        /// Get an instance of an ExpandedDomainRole, based upon the provided path parameter
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string Name</item>
        /// </list>
        /// <returns>ObjectResult holding either:
        /// <list type="bullet">
        /// <item>ExpandedDomainRole with Success status code</item>
        /// <item>null object with Not Found status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> GetExpandedAsync(string pathParameter) {
            var role = await FindExpandedAsync(pathParameter);
            if (role == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                return new ObjectResult(role) { StatusCode = StatusCodes.Status200OK };
            }
        }

        /// <summary>
        /// Gets a paged list of ExpandedDomainRoles (optionally) by application name.
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="pageNumber">Page number of recordset</param>
        /// <param name="pageSize">Page size of recordset</param>
        /// <returns>Paged list of ExpandedDomainRole records</returns>
        public async Task<ObjectResult> GetExpandedAsync(string appName = null,
            int? pageNumber = 1, int? pageSize = 100) {

            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            #region FormattableString sql
            FormattableString sql;

            if (appName != null)
                sql = $@"
select r.*
	from AspNetExpandedRoles r
	where r.Name = {appName}
    offset {skip} rows
    fetch next {take} rows only
";
            else
                sql = $@"
select r.*
	from AspNetExpandedRoles r
    offset {skip} rows
    fetch next {take} rows only
";
            #endregion
            var result = await _dbContext.ExpandedRoles
                            .FromSqlInterpolated(sql)
                            .AsNoTracking()
                            .ToListAsync();

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };
        }



        /// <summary>
        /// Creates multiple users.
        /// Note that a JSON array is used so that the model can accomodate
        /// arbitrary extra properties that get packed as a JSON Properties 
        /// column in the AspNetUsers table
        /// </summary>
        /// <param name="jsonElement">A JSON array of users to create.</param>
        /// <param name="modelState">Object for holding errors</param>
        /// <param name="sysUser">The SysUser to associate with created records</param>
        /// <returns>Null object with Status Code</returns>
        public async Task<ObjectResult> BatchCreateAsync(JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {
            if (jsonElement.ValueKind == JsonValueKind.Array) {
                ObjectResult result = null;
                foreach (var role in jsonElement.EnumerateArray()) {
                    var localResult = await CreateAsync(role, modelState, sysUser);
                    if (result == null || localResult.StatusCode >= 400)
                        result = localResult;
                }
                return result;
            } else {
                return await CreateAsync(jsonElement, modelState, sysUser);
            }
        }

        /// <summary>
        /// Creates a single user.
        /// Note that a JSON object is used so that the model can accomodate
        /// arbitrary extra properties that get packed as a JSON Properties 
        /// column in the AspNetUsers table
        /// </summary>
        /// <param name="jsonElement">A JSON object of a user to create</param>
        /// <param name="modelState">Object for holding errors</param>
        /// <param name="sysUser">The SysUser to associate with created record</param>
        /// <returns>One of the following:
        /// <list type="bullet">
        /// <item>Created DomainRole object and Success status code</item>
        /// <item>Null object and Conflict status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> CreateAsync(JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {

            var inputUser = new DomainRole();
            DeserializeInto(inputUser, jsonElement, modelState, sysUser);

            if (inputUser.Id == default)
                inputUser.Id = CombGuid.Create();

            //allow OrganizationName to be passed in and resolved to OrganizationId, when OrganizationId = default
            if (inputUser.ApplicationId == default && jsonElement.TryGetProperty("ApplicationName", out JsonElement appNameElement)) {
                var appName = appNameElement.GetString();
                try {
                    var appId = (await _dbContext.Applications.FirstOrDefaultAsync(o => o.Name == appName)).Id;
                    inputUser.ApplicationId = appId;
                } catch (Exception ex) {
                    modelState.AddModelError("ApplicationName", ex.Message);
                }
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };

            try {
                _dbContext.Add(inputUser);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                modelState.AddModelError("", ex.Message);
            }


            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(inputUser) { StatusCode = StatusCodes.Status200OK };

        }



        /// <summary>
        /// Updates a single user.
        /// Note that a JSON object is used for two reasons:
        /// <list type="bullet">
        /// <item>The model can accomodate arbitrary extra properties that 
        /// get packed as a JSON Properties column in the AspNetUsers table
        /// </item>
        /// <item>Only properties provided in the JSON element are 
        /// updated.  Hence, this allows for partial updates.
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string UserName</item>
        /// <item>string Email</item>
        /// </list>
        /// </param>
        /// <param name="jsonElement">A JSON object of a user to patch</param>
        /// <param name="modelState">Object for holding errors</param>
        /// <param name="sysUser">The SysUser to associate with patched record</param>
        /// <returns>One of the following:
        /// <list type="bullet">
        /// <item>Updated DomainRole object and Success status code</item>
        /// <item>Null object and Conflict status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> PatchAsync(string pathParameter, JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {


            var existingUser = await FindAsync(pathParameter);

            if (existingUser == null) {
                modelState.AddModelError("pathParameter", $"A user designated by '{pathParameter}' does not exist.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status404NotFound };
            }

            DeserializeInto(existingUser, jsonElement, modelState, sysUser);

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };

            try {
                _dbContext.Update(existingUser);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                modelState.AddModelError("", ex.Message);
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(existingUser) { StatusCode = StatusCodes.Status200OK };

        }

        /// <summary>
        /// Deletes a single user record.  Note that the record's
        /// SysStatus is updated to Deleted and saved before deleting, which
        /// ensures an audit trail of the person who deleted the record
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string UserName</item>
        /// <item>string Email</item>
        /// </list>
        /// </param>
        /// <param name="modelState">Object for holding errors</param>
        /// <param name="sysUser">The SysUser to associate with deleted record</param>
        /// <returns></returns>
        public async Task<ObjectResult> DeleteAsync(string pathParameter, ModelStateDictionary modelState, string sysUser) {

            var existingUser = await FindAsync(pathParameter);

            if (existingUser == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };


            //first, try to update the record with a Deleted status and the deleting user;
            //however, just ignore if there is an error.
            try {
                existingUser.SysStatus = SysStatus.Deleted;
                existingUser.SysUser = sysUser;
                _dbContext.Update(existingUser);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                modelState.AddModelError("", ex.Message);
            }


            //second, actually delete the record
            try {
                _dbContext.Remove(existingUser);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                modelState.AddModelError("", ex.Message);
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };

        }




        #region Private Helper Methods

        /// <summary>
        /// Finds DomainRoles by the provided path parameter
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string UserName</item>
        /// <item>string Email</item>
        /// </list>
        /// <returns>DomainRole record</returns>
        private async Task<DomainRole> FindAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _dbContext.Set<DomainRole>()
                    .SingleAsync(u => u.Id == Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<DomainRole>()
                    .SingleAsync(u => u.Name == pathParameter);
        }

        /// <summary>
        /// Finds ExpandedDomainRoles by the provided path parameter
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string UserName</item>
        /// <item>string Email</item>
        /// </list>
        /// <returns>ExpandedDomainRole record</returns>
        private async Task<OLDExpandedDomainRole> FindExpandedAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _dbContext.Set<OLDExpandedDomainRole>()
                    .SingleAsync(u => u.Id == Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<OLDExpandedDomainRole>()
                    .SingleAsync(u => u.Name == pathParameter);
        }

        /// <summary>
        /// Deserializes the provided JsonElement into the referenced DomainRole object
        /// </summary>
        /// <param name="user">Referenced DomainRole object (new or existing)</param>
        /// <param name="jsonElement">Parsed JSON object</param>
        /// <param name="modelState">Object to hold errors</param>
        /// <param name="sysUser">SysUser to update in user record</param>
        private void DeserializeInto(DomainRole role, JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {
            OtherProperties otherProperties = null;
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Name":
                        case "name":
                            role.Name = prop.Value.GetString();
                            role.NormalizedName = role.Name.ToUpper();
                            break;
                        case "NormalizedName":
                        case "normalizedName":
                            role.NormalizedName = prop.Value.GetString();
                            break;
                        case "Id":
                        case "id":
                            role.Id = prop.Value.GetGuid();
                            break;
                        case "SysUser":
                        case "sysUser":
                            role.SysUser = sysUser;
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            role.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            role.SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            role.SysEnd = prop.Value.GetDateTime();
                            break;
                        case "ApplicationId":
                        case "applicationId":
                            role.ApplicationId = prop.Value.GetGuid();
                            break;
                        case "ConcurrencyStamp":
                        case "concurrencyStamp":
                        case "SecurityStamp":
                        case "securityStamp":
                        case "LockoutEnabled":
                        case "lockoutEnabled":
                        case "ApplicationName":
                        case "applicationName":
                        default:
                            if (otherProperties == null)
                                otherProperties = new OtherProperties();
                            otherProperties.Add(prop);
                            break;
                    }
                } catch (Exception ex) {
                    modelState.AddModelError(prop.Name, $"Parsing error: {ex.Message}");
                }
            }
            role.Properties = otherProperties.ToString();
        }

        #endregion

    }

}