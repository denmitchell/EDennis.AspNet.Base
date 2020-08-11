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
using System.Threading.Tasks;


namespace EDennis.NetStandard.Base {
    public class DomainUserRepo {

        public DomainIdentityDbContext _dbContext;


        public DomainUserRepo(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }



        /// <summary>
        /// Get an instance of a DomainUser, based upon the path parameter
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string UserName</item>
        /// <item>string Email</item>
        /// </list>
        /// <returns>ObjectResult holding either:
        /// <list type="bullet">
        /// <item>DomainUser with Success status code</item>
        /// <item>null object with Not Found status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var user = await FindAsync(pathParameter);
            if (user == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                return new ObjectResult(user) { StatusCode = StatusCodes.Status200OK };
            }
        }

        /// <summary>
        /// Gets a paged list of DomainUsers by application name, organization name, 
        /// both application name and organization name, or neither application
        /// name nor organization name.
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="orgName">Organization Name</param>
        /// <param name="pageNumber">Page number of recordset</param>
        /// <param name="pageSize">Page size of recordset</param>
        /// <returns>Paged list of DomainUser records</returns>
        public async Task<ObjectResult> GetAsync(string appName = null, string orgName = null,
            int? pageNumber = 1, int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            #region FormattableString sql
            FormattableString sql;

            if (appName != null && orgName != null)
                sql = $@"
select u.*
	from AspNetUsers u
	where
        OrganizationName = {orgName} and
        exists (
		select 0 
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = r.ApplicationId
			inner join AspNetUserRoles ur
				on r.Id = ur.RoleId
			where a.Name = {appName}
				and ur.UserId = u.Id
	)    offset {skip} rows
    fetch next {take} rows only
";
            else if (appName != null)
                sql = $@"
select u.*
	from AspNetUsers u
	where
        exists (
		select 0 
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = r.ApplicationId
			inner join AspNetUserRoles ur
				on r.Id = ur.RoleId
			where a.Name = {appName}
				and ur.UserId = u.Id
	)    offset {skip} rows
    fetch next {take} rows only
";
            else if (orgName != null)
                sql = $@"
select u.*
	from AspNetUsers u
	where
        OrganizationName = {orgName}
    offset {skip} rows
    fetch next {take} rows only
";
            else
                sql = $@"
select u.*
	from AspNetUsers u
    offset {skip} rows
    fetch next {take} rows only
";
            #endregion
            var result = await _dbContext.Set<DomainUser>()
                            .FromSqlInterpolated(sql)
                            .AsNoTracking()
                            .ToListAsync();
                                               
            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };
        }



        /// <summary>
        /// Get an instance of an ExpandedDomainUser, based upon the provided path parameter
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string UserName</item>
        /// <item>string Email</item>
        /// </list>
        /// <returns>ObjectResult holding either:
        /// <list type="bullet">
        /// <item>ExpandedDomainUser with Success status code</item>
        /// <item>null object with Not Found status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> GetExpandedAsync(string pathParameter) {
            var user = await FindViewAsync(pathParameter);
            if (user == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                return new ObjectResult(user) { StatusCode = StatusCodes.Status200OK };
            }
        }

        /// <summary>
        /// Gets a paged list of ExpandedDomainUsers by application name, organization name, 
        /// both application name and organization name, or neither application
        /// name nor organization name.
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="orgName">Organization Name</param>
        /// <param name="pageNumber">Page number of recordset</param>
        /// <param name="pageSize">Page size of recordset</param>
        /// <returns>Paged list of ExpandedDomainUser records</returns>
        public async Task<ObjectResult> GetExpandedAsync(string appName = null, string orgName = null,
            int? pageNumber = 1, int? pageSize = 100) {

            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            #region FormattableString sql
            FormattableString sql;

            if (appName != null && orgName != null)
                sql = $@"
select u.*
	from AspNetExpandedUsers u
	where
        OrganizationName = {orgName} and
        exists (
		select 0 
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = r.ApplicationId
			inner join AspNetUserRoles ur
				on r.Id = ur.RoleId
			where a.Name = {appName}
				and ur.UserId = u.Id
	)    offset {skip} rows
    fetch next {take} rows only
";
            else if (appName != null)
                sql = $@"
select u.*
	from AspNetExpandedUsers u
	where
        exists (
		select 0 
			from AspNetRoles r
			inner join AspNetApplications a
				on a.Id = r.ApplicationId
			inner join AspNetUserRoles ur
				on r.Id = ur.RoleId
			where a.Name = {appName}
				and ur.UserId = u.Id
	)    offset {skip} rows
    fetch next {take} rows only
";
            else if (orgName != null)
                sql = $@"
select u.*
	from AspNetExpandedUsers u
	where
        OrganizationName = {orgName}
    offset {skip} rows
    fetch next {take} rows only
";
            else
                sql = $@"
select u.*
	from AspNetExpandedUsers u
    offset {skip} rows
    fetch next {take} rows only
";
            #endregion
            var result = await _dbContext.Set<DomainUserView>()
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
                foreach(var user in jsonElement.EnumerateArray()) {
                    var localResult = await CreateAsync(user, modelState, sysUser);
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
        /// <item>Created DomainUser object and Success status code</item>
        /// <item>Null object and Conflict status code</item>
        /// </list>
        /// </returns>
        public async Task<ObjectResult> CreateAsync(JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {

            var inputUser = new DomainUser();
            DeserializeInto(inputUser, jsonElement, modelState, sysUser);

            //allow OrganizationName to be passed in and resolved to OrganizationId, when OrganizationId = default
            if (inputUser.OrganizationId == default && jsonElement.TryGetProperty("OrganizationName", out JsonElement orgNameElement)) {
                var orgName = orgNameElement.GetString();
                try {
                    var orgId = (await _dbContext.Set<DomainOrganization>().FirstOrDefaultAsync(o => o.Name == orgName)).Id;
                    inputUser.OrganizationId = orgId;
                } catch (Exception ex) {
                    modelState.AddModelError("", ex.Message);
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
        /// <item>Updated DomainUser object and Success status code</item>
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



        /// <summary>
        /// For a given application, updates roles for all users.  Note that base records
        /// for users and roles must already exist in the database.  Only UserRole
        /// records are updated.
        /// </summary>
        /// <param name="appName">The name of the application whose user roles should be updated</param>
        /// <param name="userRoles">Dictionary where UserName is the key and List of RoleNames is the value</param>
        /// <param name="modelState">object to hold errors</param>
        /// <param name="sysUser">the SysUser to be associated with the update</param>
        /// <returns></returns>
        public async Task UpdateDictionaryUserRolesForAppUsers(string appName,
                Dictionary<string,List<string>> userRoles, string sysUser) {


            var updatedRoles = userRoles.SelectMany(entry => entry.Value.Select(role => 
                    new UserNameRoleName{ UserName = entry.Key, RoleName = role }));

            SqlConnection cxn = null;
            try {
                cxn = _dbContext.Database.GetDbConnection() as SqlConnection;
            } catch (Exception ex) {
                throw new ApplicationException($"DomainUserRepo.UpdateUserRolesForAppUsers only works for SQL Server connections: {ex.Message}");
            }

            SqlCommand cmd = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "di.UpdateUserRolesForAppUsers",
                Connection = cxn
            };

            if (_dbContext.Database.CurrentTransaction != null) {
                var trans = _dbContext.Database.CurrentTransaction.GetDbTransaction() as SqlTransaction;
                cmd.Transaction = trans;
            }

            SqlParameter pAppName = cmd.Parameters.AddWithValue("@AppName", appName);
            SqlParameter pSysUser = cmd.Parameters.AddWithValue("@SysUser", sysUser);
            SqlParameter pUserRoles = cmd.Parameters.AddWithValue("@UserRoles", updatedRoles);
            pUserRoles.SqlDbType = SqlDbType.Structured;
            pUserRoles.TypeName = "di.UserRolesType";

            await cmd.ExecuteNonQueryAsync();

        }

        /// <summary>
        /// For a given application, updates roles for all users.  Note that base records
        /// for users and roles must already exist in the database.  Only UserRole
        /// records are updated.
        /// </summary>
        /// <param name="appName">The name of the application whose user roles should be updated</param>
        /// <param name="userRoles">Collection of updated DomainUserRole records for the application</param>
        /// <param name="modelState">object to hold errors</param>
        /// <param name="sysUser">the SysUser to be associated with the update</param>
        /// <returns></returns>
        public async Task UpdateDomainUserRolesForAppUsers(string appName,
                ICollection<DomainUserRole> userRoles, string sysUser) {

            SqlConnection cxn;
            try {
                cxn = _dbContext.Database.GetDbConnection() as SqlConnection;
            } catch (Exception ex) {
                throw new ApplicationException($"DomainUserRepo.UpdateDomainUserRolesForAppUsers only works for SQL Server connections: {ex.Message}");
            }

            SqlCommand cmd = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "di.UpdateDomainUserRolesForAppUsers",
                Connection = cxn
            };

            if (_dbContext.Database.CurrentTransaction != null) {
                var trans = _dbContext.Database.CurrentTransaction.GetDbTransaction() as SqlTransaction;
                cmd.Transaction = trans;
            }

            cmd.Parameters.AddWithValue("@AppName", appName);
            cmd.Parameters.AddWithValue("@SysUser", sysUser);

            SqlParameter pUserRoles = cmd.Parameters.AddWithValue("@DomainUserRoles", userRoles);
            pUserRoles.SqlDbType = SqlDbType.Structured;
            pUserRoles.TypeName = "di.DomainUserRolesType";

            await cmd.ExecuteNonQueryAsync();

        }



        /// <summary>
        /// Updates all application roles for a given user.  Note that base records
        /// for users and roles must already exist in the database.  Only UserRole
        /// records are updated.
        /// </summary>
        /// <param name="userName">The name of the user whose user roles should be updated</param>
        /// <param name="appRoles">Dictionary where ApplicationName is the key and List of RoleNames is the value</param>
        /// <param name="sysUser">the SysUser to be associated with the update</param>
        /// <returns></returns>
        public async Task UpdateDictionaryUserRolesForUser(string userName,
                Dictionary<string, List<string>> appRoles, string sysUser) {


            var updatedRoles = appRoles.SelectMany(entry => entry.Value.Select(role =>
                    new ApplicationNameRoleName { ApplicationName = entry.Key, RoleName = role }));

            SqlConnection cxn = null;
            try {
                cxn = _dbContext.Database.GetDbConnection() as SqlConnection;
            } catch (Exception ex) {
                throw new ApplicationException($"DomainUserRepo.UpdateUserRolesForUser only works for SQL Server connections: {ex.Message}");
            }

            SqlCommand cmd = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "di.UpdateUserRolesForUser",
                Connection = cxn
            };

            if (_dbContext.Database.CurrentTransaction != null) {
                var trans = _dbContext.Database.CurrentTransaction.GetDbTransaction() as SqlTransaction;
                cmd.Transaction = trans;
            }

            SqlParameter pAppName = cmd.Parameters.AddWithValue("@UserName", userName);
            SqlParameter pSysUser = cmd.Parameters.AddWithValue("@SysUser", sysUser);
            SqlParameter pUserRoles = cmd.Parameters.AddWithValue("@AppRoles", updatedRoles);
            pUserRoles.SqlDbType = SqlDbType.Structured;
            pUserRoles.TypeName = "di.AppRolesType";

            await cmd.ExecuteNonQueryAsync();

        }

        /// <summary>
        /// Updates all application roles for a given user.  Note that base records
        /// for users and roles must already exist in the database.  Only UserRole
        /// records are updated.
        /// </summary>
        /// <param name="userName">The name of the user whose user roles should be updated</param>
        /// <param name="appRoles">Collection of updated DomainUserRole records for the user</param>
        /// <param name="sysUser">the SysUser to be associated with the update</param>
        /// <returns></returns>
        public async Task UpdateDomainUserRolesForUser(string userName,
                ICollection<DomainUserRole> appRoles, string sysUser) {


            SqlConnection cxn;
            try {
                cxn = _dbContext.Database.GetDbConnection() as SqlConnection;
            } catch (Exception ex) {
                throw new ApplicationException($"DomainUserRepo.UpdateDomainUserRolesForUser only works for SQL Server connections: {ex.Message}");
            }

            SqlCommand cmd = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "di.UpdateDomainUserRolesForUser",
                Connection = cxn
            };

            if (_dbContext.Database.CurrentTransaction != null) {
                var trans = _dbContext.Database.CurrentTransaction.GetDbTransaction() as SqlTransaction;
                cmd.Transaction = trans;
            }

            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@SysUser", sysUser);
            SqlParameter pUserRoles = cmd.Parameters.AddWithValue("@DomainUserRoles", appRoles);
            pUserRoles.SqlDbType = SqlDbType.Structured;
            pUserRoles.TypeName = "di.DomainUserRolesType";

            await cmd.ExecuteNonQueryAsync();

        }



        /// <summary>
        /// Updates all claims for a given user.  Note that base record
        /// for user must already exist in the database.  Only UserClaim
        /// records are updated.
        /// </summary>
        /// <param name="userName">The name of the user whose claims should be updated</param>
        /// <param name="claims">Dictionary where ClaimType is the key and List of ClaimValue is the value</param>
        /// <param name="sysUser">the SysUser to be associated with the update</param>
        /// <returns></returns>
        public async Task UpdateDictionaryUserClaims(string userName,
                Dictionary<string, List<string>> claims, string sysUser) {


            var updatedClaims = claims.SelectMany(entry => entry.Value.Select(value =>
                    new ClaimTypeClaimValue { ClaimType = entry.Key, ClaimValue = value }));

            SqlConnection cxn = null;
            try {
                cxn = _dbContext.Database.GetDbConnection() as SqlConnection;
            } catch (Exception ex) {
                throw new ApplicationException($"DomainUserRepo.UpdateUserClaims only works for SQL Server connections: {ex.Message}");
            }

            SqlCommand cmd = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "di.UpdateUserClaims",
                Connection = cxn
            };

            if (_dbContext.Database.CurrentTransaction != null) {
                var trans = _dbContext.Database.CurrentTransaction.GetDbTransaction() as SqlTransaction;
                cmd.Transaction = trans;
            }

            SqlParameter pAppName = cmd.Parameters.AddWithValue("@UserName", userName);
            SqlParameter pSysUser = cmd.Parameters.AddWithValue("@SysUser", sysUser);
            SqlParameter pUserRoles = cmd.Parameters.AddWithValue("@Claims", updatedClaims);
            pUserRoles.SqlDbType = SqlDbType.Structured;
            pUserRoles.TypeName = "di.ClaimsType";

            await cmd.ExecuteNonQueryAsync();

        }

        /// <summary>
        /// Updates all claims for a given user.  Note that base record
        /// for user must already exist in the database.  Only UserClaim
        /// records are updated.
        /// </summary>
        /// <param name="userName">The name of the user whose claims should be updated</param>
        /// <param name="claims">Collection of updated DomainUserClaim records for the user</param>
        /// <param name="sysUser">the SysUser to be associated with the update</param>
        /// <returns></returns>
        public async Task UpdateDomainUserClaims(string userName,
                IEnumerable<DomainUserClaim> claims, string sysUser) {


            SqlConnection cxn;
            try {
                cxn = _dbContext.Database.GetDbConnection() as SqlConnection;
            } catch (Exception ex) {
                throw new ApplicationException($"DomainUserRepo.UpdateDomainUserClaims only works for SQL Server connections: {ex.Message}");
            }

            SqlCommand cmd = new SqlCommand {
                CommandType = CommandType.StoredProcedure,
                CommandText = "di.UpdateDomainUserClaims",
                Connection = cxn
            };

            if (_dbContext.Database.CurrentTransaction != null) {
                var trans = _dbContext.Database.CurrentTransaction.GetDbTransaction() as SqlTransaction;
                cmd.Transaction = trans;
            }

            cmd.Parameters.AddWithValue("@UserName", userName);
            cmd.Parameters.AddWithValue("@SysUser", sysUser);
            SqlParameter pUserRoles = cmd.Parameters.AddWithValue("@DomainUserClaims", claims);
            pUserRoles.SqlDbType = SqlDbType.Structured;
            pUserRoles.TypeName = "di.DomainUserClaimsType";

            await cmd.ExecuteNonQueryAsync();

        }



        #region Private Helper Methods

        /// <summary>
        /// Finds DomainUser records by the provided path parameter
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>int Id</item>
        /// <item>string UserName</item>
        /// <item>string Email</item>
        /// </list>
        /// <returns>DomainUser record</returns>
        private async Task<DomainUser> FindAsync(string pathParameter) {

            if (int.TryParse(pathParameter, out int id))
                return await _dbContext.Set<DomainUser>().SingleAsync(e => e.Id == id);
            else if (pathParameter.Contains("@"))
                return await _dbContext.Set<DomainUser>().SingleAsync(e => e.NormalizedEmail == pathParameter.ToUpper());
            else
                return await _dbContext.Set<DomainUser>().SingleAsync(a => a.NormalizedUserName == pathParameter.ToUpper());
        }

        /// <summary>
        /// Finds DomainUserView records by the provided path parameter
        /// </summary>
        /// <param name="pathParameter">One of the following:
        /// <list type="bullet">
        /// <item>int Id</item>
        /// <item>string UserName</item>
        /// <item>string Email</item>
        /// </list>
        /// <returns>ExpandedDomainUser record</returns>
        private async Task<DomainUserView> FindViewAsync(string pathParameter) {
            if (int.TryParse(pathParameter, out int id))
                return await _dbContext.Set<DomainUserView>().SingleAsync(e => e.Id == id);
            else if (pathParameter.Contains("@"))
                return await _dbContext.Set<DomainUserView>().SingleAsync(e => e.NormalizedEmail == pathParameter.ToUpper());
            else
                return await _dbContext.Set<DomainUserView>().SingleAsync(a => a.NormalizedUserName == pathParameter.ToUpper());
        }

        /// <summary>
        /// Deserializes the provided JsonElement into the referenced DomainUser object
        /// </summary>
        /// <param name="user">Referenced DomainUser object (new or existing)</param>
        /// <param name="jsonElement">Parsed JSON object</param>
        /// <param name="modelState">Object to hold errors</param>
        /// <param name="sysUser">SysUser to update in user record</param>
        private void DeserializeInto(DomainUser user, JsonElement jsonElement, 
            ModelStateDictionary modelState, string sysUser) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "UserName":
                        case "userName":
                            user.UserName = prop.Value.GetString();
                            user.NormalizedUserName ??= user.UserName.ToUpper();
                            break;
                        case "NormalizedUserName":
                        case "normalizedUserName":
                            user.NormalizedUserName = prop.Value.GetString();
                            break;
                        case "Id":
                        case "id":
                            user.Id = prop.Value.GetInt32();
                            break;
                        case "AccessFailedCount":
                        case "accessFailedCount":
                            user.AccessFailedCount = prop.Value.GetInt32();
                            break;
                        case "Email":
                        case "email":
                            user.Email = prop.Value.GetString();
                            user.NormalizedEmail ??= user.Email.ToUpper();
                            break;
                        case "NormalizedEmail":
                        case "normalizedEmail":
                            user.NormalizedEmail = prop.Value.GetString();
                            break;
                        case "EmailConfirmed":
                        case "emailConfirmed":
                            user.EmailConfirmed = prop.Value.GetBoolean();
                            break;
                        case "LockoutBegin":
                        case "lockoutBegin":
                            user.LockoutBegin = prop.Value.GetDateTimeOffset();
                            break;
                        case "LockoutEnd":
                        case "lockoutEnd":
                            user.LockoutEnd = prop.Value.GetDateTimeOffset();
                            break;
                        case "PhoneNumber":
                        case "phoneNumber":
                            user.PhoneNumber = prop.Value.GetString();
                            break;
                        case "PhoneNumberConfirmed":
                        case "phoneNumberConfirmed":
                            user.PhoneNumberConfirmed = prop.Value.GetBoolean();
                            break;
                        case "TwoFactorEnabled":
                        case "twoFactorEnabled":
                            user.TwoFactorEnabled = prop.Value.GetBoolean();
                            break;
                        case "SysUser":
                        case "sysUser":
                            user.SysUser = sysUser;
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            user.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            user.SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            user.SysEnd = prop.Value.GetDateTime();
                            break;
                        case "PasswordHash":
                        case "passwordHash":
                            var pwd = prop.Value.GetString();
                            if (pwd.Length == DomainUser.SHA256_LENGTH || pwd.Length == DomainUser.SHA512_LENGTH)
                                user.PasswordHash = pwd;
                            else
                                user.PasswordHash = pwd.ToSha256();
                            break;
                        case "OrganizationId":
                        case "organizationId":
                            user.OrganizationId = prop.Value.GetInt32();
                            break;
                    }
                } catch (Exception ex) {
                    modelState.AddModelError(prop.Name, $"Parsing error: {ex.Message}");
                }
            }
        }

        #endregion

    }

}