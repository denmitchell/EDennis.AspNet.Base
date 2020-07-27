using EDennis.AspNet.Base.Security.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading.Tasks;
using IdentityModel;
using EDennis.AspNet.Base.Security.AspNetIdentityServer.Models.JsonConverters;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserRepo {

        public DomainIdentityDbContext _dbContext;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");
        public static Regex lpPattern = new Regex("(?:\\(\\s*loginProvider\\s*=\\s*'?)([^,']+)(?:'?\\s*,\\s*providerKey\\s*=\\s*'?)([^)']+)(?:'?\\s*\\))");


        public DomainUserRepo(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }



        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var user = await FindAsync(pathParameter);
            if (user == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentClaims = (await _userManager.GetClaimsAsync(user)).ToStringDictionary();

                var userEditModel = user.ToEditModel();
                userEditModel.Roles = currentRoles;
                userEditModel.Claims = currentClaims;

                return new ObjectResult(userEditModel) { StatusCode = StatusCodes.Status200OK };
            }
        }





        public async Task<ObjectResult> GetAsync(string appName = null, string orgName = null,
            int? pageNumber = 1, int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            var qry = _dbContext.Users as IQueryable<DomainUser>;

            if (appName != null)
                qry = qry.Where(u => u.UserRoles.Any(r => r.Role.Application.Name == appName));

            if (orgName != null)
                qry = qry.Where(u => u.Organization.Name == orgName);

            qry = qry.Skip(skip)
                .Take(take)
                .AsNoTracking();

            var result = (await qry.ToListAsync()).Select(x => x.ToEditModel());

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };

        }



        private async Task<DomainUser> FindAsync(string pathParameter) {
            if (pathParameter.Contains("@"))
                return await _dbContext.Set<DomainUser>().SingleAsync(u => u.NormalizedEmail == pathParameter.ToUpper());

            else if (idPattern.IsMatch(pathParameter))
                return await _dbContext.Set<DomainUser>().SingleAsync(u => u.Id == Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<DomainUser>().SingleAsync(u => u.NormalizedUserName == pathParameter.ToUpper());
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


            /*

            if (inputUser.Roles != null && inputUser.Roles.Count() > 0)
                results.Add(await _userManager.AddToRolesAsync(user, inputUser.Roles));

            if (inputUser.Claims != null && inputUser.Claims.Count() > 0)
                results.Add(await _userManager.AddClaimsAsync(user, inputUser.Claims.ToClaimEnumerable()));


            results.SelectMany(r => r.Errors).ToList()
                .ForEach(e => modelState.AddModelError("", e.Description));

            */

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(inputUser) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> PatchAsync(string name, JsonElement jsonElement,
            ModelStateDictionary modelState, string sysUser) {


            var existingUser = _dbContext.Set<DomainUser>().FirstOrDefault(u => u.UserName == name);

            if (existingUser == null) {
                modelState.AddModelError("Name", $"A user with Name/UserName='{name}' does not exist.");
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

            /*
            results.Add(await _userManager.UpdateAsync(existingUser));

            results.SelectMany(r => r.Errors).ToList()
                .ForEach(e=>modelState.AddModelError("",e.Description));
            */

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(existingUser) { StatusCode = StatusCodes.Status200OK };

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



    }



}