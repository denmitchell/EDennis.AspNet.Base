using EDennis.AspNet.Base.Security.AspNetIdentityServer.Models.JsonConverters;
using EDennis.AspNet.Base.Security.Extensions;
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


namespace EDennis.AspNet.Base.Security {
    public class DomainUserRepo {

        public DomainIdentityDbContext _dbContext;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");
        public static Regex lpPattern = new Regex("(?:\\(\\s*loginProvider\\s*=\\s*'?)([^,']+)(?:'?\\s*,\\s*providerKey\\s*=\\s*'?)([^)']+)(?:'?\\s*\\))");


        public DomainUserRepo(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }



        /// <summary>
        /// Get an instance of an ExpandedDomainUser, based upon either:
        /// <list type="bullet">
        /// <item>GUID Id</item>
        /// <item>string Email</item>
        /// <item>string UserName</item>
        /// </list>
        /// </summary>
        /// <param name="pathParameter">Id, Email, or UserName</param>
        /// <returns>ExpandedDomainUser</returns>
        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var user = await FindAsync(pathParameter);
            if (user == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                return new ObjectResult(user) { StatusCode = StatusCodes.Status200OK };
            }
        }


        /// <summary>
        /// Gets a paged list of users by application name, organization name, 
        /// both application name and organization name, or neither application
        /// name nor organization name.
        /// </summary>
        /// <param name="appName">Application Name</param>
        /// <param name="orgName">Organization Name</param>
        /// <param name="pageNumber">Page number of recordset</param>
        /// <param name="pageSize">Page size of recordset</param>
        /// <returns></returns>
        public async Task<ObjectResult> GetAsync(string appName = null, string orgName = null,
            int? pageNumber = 1, int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            #region FormattableString sql
            FormattableString sql;

            if (appName != null && orgName != null)
                sql = $@"
select u.*
	from @ExpandedUsers u
	where
        OrganizationName = {orgName} and
        exists (
		select 0 
			from @Roles r
			inner join @Applications a
				on a.Id = r.ApplicationId
			inner join @UserRoles ur
				on r.Id = ur.RoleId
			where a.Name = {appName}
				and ur.UserId = u.Id
	)    offset {skip} rows
    fetch next {take} rows only
";
            else if (appName != null)
                sql = $@"
select u.*
	from @ExpandedUsers u
	where
        exists (
		select 0 
			from @Roles r
			inner join @Applications a
				on a.Id = r.ApplicationId
			inner join @UserRoles ur
				on r.Id = ur.RoleId
			where a.Name = {appName}
				and ur.UserId = u.Id
	)    offset {skip} rows
    fetch next {take} rows only
";
            else if (orgName != null)
                sql = $@"
select u.*
	from @ExpandedUsers u
	where
        OrganizationName = {orgName}
    offset {skip} rows
    fetch next {take} rows only
";
            else
                sql = $@"
select u.*
	from @ExpandedUsers u
    offset {skip} rows
    fetch next {take} rows only
";
            #endregion
            var result = await _dbContext.ExpandedUsers
                            .FromSqlInterpolated(sql)
                            .AsNoTracking()
                            .ToListAsync();
                                               
            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };
        }



        private async Task<ExpandedDomainUser> FindAsync(string pathParameter) {
            if (pathParameter.Contains("@"))
                return await _dbContext.Set<ExpandedDomainUser>()
                    .SingleAsync(u => u.NormalizedEmail == pathParameter.ToUpper());

            else if (idPattern.IsMatch(pathParameter))
                return await _dbContext.Set<ExpandedDomainUser>()
                    .SingleAsync(u => u.Id == Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<ExpandedDomainUser>()
                    .SingleAsync(u => u.NormalizedUserName == pathParameter.ToUpper());
        }



        public async Task<ObjectResult> CreateAsync(JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {

            var inputUser = new DomainUser();
            DeserializeInto(inputUser, jsonElement, modelState, sysUser);


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



        public async Task<ObjectResult> PatchAsync(string pathParameter, JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {


            var existingUser = await FindAsync(pathParameter);

            if (existingUser == null) {
                modelState.AddModelError("Name", $"A user designated by '{pathParameter}' does not exist.");
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

        public async Task<ObjectResult> DeleteAsync(string name, ModelStateDictionary modelState,
            string sysUser) {

            var existingUser = _dbContext.Set<DomainUser>().FirstOrDefault(u => u.UserName == name);

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
        public async Task UpdateUserRolesForAppUsers(string appName,
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
        public async Task UpdateUserRolesForUser(string userName,
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
        public async Task UpdateUserClaims(string userName,
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



        private void DeserializeInto(DomainUser user, JsonElement jsonElement, ModelStateDictionary modelState, string sysUser) {
            OtherProperties otherProperties = null;
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "UserName":
                        case "userName":
                            user.UserName = prop.Value.GetString();
                            user.NormalizedUserName ??= user.UserName.ToUpper();
                            var existingUser = _dbContext.Set<DomainUser>().FirstOrDefault(u => u.UserName == user.UserName);

                            if (existingUser != null) {
                                modelState.AddModelError("Name", $"A user with Name/UserName='{user.UserName}' already exists.");
                            }
                            break;
                        case "NormalizedUserName":
                        case "normalizedUserName":
                            user.NormalizedUserName = prop.Value.GetString();
                            break;
                        case "Id":
                        case "id":
                            user.Id = prop.Value.GetGuid();
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
                            prop.Value.GetInt32();
                            break;
                        case "ConcurrencyStamp":
                        case "concurrencyStamp":
                        case "SecurityStamp":
                        case "securityStamp":
                        case "LockoutEnabled":
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
            user.Properties = otherProperties.ToString();
        }

    }

}