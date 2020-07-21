using EDennis.AspNet.Base.EntityFramework;
using EDennis.AspNet.Base.Security.AspNetIdentityServer.Models.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserRepo {

        public DomainIdentityDbContext _dbContext;
        public UserManager<DomainUser> _userManager;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");
        public static Regex lpPattern = new Regex("(?:\\(\\s*loginProvider\\s*=\\s*'?)([^,']+)(?:'?\\s*,\\s*providerKey\\s*=\\s*'?)([^)']+)(?:'?\\s*\\))");


        public DomainUserRepo(DbContextProvider<DomainIdentityDbContext> provider, UserManager<DomainUser> userManager) {
            _dbContext = provider.DbContext;
            _userManager = userManager;
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
                
            var result = (await qry.ToListAsync()).Select(x=>x.ToEditModel());

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };

        }



        private async Task<DomainUser> FindAsync(string pathParameter) {
            if (pathParameter.Contains("@"))
                return await _userManager.FindByEmailAsync(pathParameter);
            else if (idPattern.IsMatch(pathParameter))
                return await _userManager.FindByIdAsync(pathParameter);
            else
                return await _userManager.FindByNameAsync(pathParameter);
        }


        public async Task<ObjectResult> CreateAsync(UserEditModel userEditModel, ModelStateDictionary modelState) {


            var existingUser = _dbContext.Set<DomainUser>().FirstOrDefault(u => u.UserName == userEditModel.Name);

            if (existingUser != null) {
                modelState.AddModelError("Name", $"A user with Name/UserName='{userEditModel.Name}' already exists.");
            }

            DomainOrganization existingOrganization;

            if (userEditModel.Organization != null) {
                existingOrganization = _dbContext.Set<DomainOrganization>().FirstOrDefault(o => o.Name == userEditModel.Organization);

                if (existingOrganization == null) {
                    modelState.AddModelError("Organization", $"An organization with Name ='{userEditModel.Organization}' does not exists.");
                }
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };


            var user = new DomainUser {
                Id = Guid.NewGuid(),
                Email = userEditModel.Email,
                NormalizedEmail = userEditModel.Email.ToUpper(),
                UserName = userEditModel.Name,
                NormalizedUserName = userEditModel.Name.ToUpper()
            };
            if (userEditModel.Password.Length == DomainUser.SHA256_LENGTH || userEditModel.Password.Length == DomainUser.SHA512_LENGTH)
                user.PasswordHash = userEditModel.Password;
            else
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userEditModel.Password);


            var results = new List<IdentityResult> {
                await _userManager.CreateAsync(user)
            };


            if (userEditModel.Roles != null && userEditModel.Roles.Count() > 0)
                results.Add(await _userManager.AddToRolesAsync(user, userEditModel.Roles));

            if (userEditModel.Claims != null && userEditModel.Claims.Count() > 0)
                results.Add(await _userManager.AddClaimsAsync(user, userEditModel.Claims.ToClaimEnumerable()));

            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return new ObjectResult(failures) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(userEditModel) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> PatchAsync(string name, JsonElement jsonElement, ModelStateDictionary modelState) {


            var existingUser = _dbContext.Set<DomainUser>().FirstOrDefault(u => u.UserName == name);

            if (existingUser == null) {
                modelState.AddModelError("Name", $"A user with Name/UserName='{name}' does not exist.");
            }

            var results = new List<IdentityResult>();
            var userEditModel = existingUser.ToEditModel();


            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Organization":
                        case "organization":
                            userEditModel.Organization = prop.Value.GetString();
                            DomainOrganization existingOrganization = null;

                            if (userEditModel.Organization != null) {
                                existingOrganization = _dbContext.Set<DomainOrganization>().FirstOrDefault(o => o.Name == userEditModel.Organization);

                                if (existingOrganization == null) {
                                    modelState.AddModelError("Organization", $"An organization with Name ='{userEditModel.Organization}' does not exists.");
                                }
                            }
                            existingUser.Organization = existingOrganization;
                            break;
                        case "Roles":
                        case "roles":
                            userEditModel.Roles = prop.Value.EnumerateArray().Select(x => x.GetString());
                            var existingRoles = await _userManager.GetRolesAsync(existingUser);
                            var rolesToAdd = userEditModel.Roles.Except(existingRoles);
                            var rolesToRemove = existingRoles.Except(userEditModel.Roles);

                            results.Add(await _userManager.AddToRolesAsync(existingUser, rolesToAdd));
                            results.Add(await _userManager.RemoveFromRolesAsync(existingUser, rolesToRemove));
                            break;
                        case "Claims":
                        case "claims":
                            userEditModel.Claims = JsonSerializer.Deserialize<Dictionary<string, string[]>>(prop.Value.GetRawText());
                            var newClaims = userEditModel.Claims.ToClaimEnumerable();
                            var existingClaims = await _userManager.GetClaimsAsync(existingUser);
                            var claimsToAdd = newClaims.Except(existingClaims);
                            var claimsToRemove = existingClaims.Except(newClaims);

                            results.Add(await _userManager.AddClaimsAsync(existingUser, claimsToAdd));
                            results.Add(await _userManager.RemoveClaimsAsync(existingUser, claimsToRemove));
                            break;
                        case "AccessFailedCount":
                        case "accessFailedCount":
                            userEditModel.AccessFailedCount = prop.Value.GetInt32();
                            existingUser.AccessFailedCount = userEditModel.AccessFailedCount;
                            break;
                        case "Email":
                        case "email":
                            userEditModel.Email = prop.Value.GetString();
                            existingUser.Email = userEditModel.Email;
                            break;
                        case "EmailConfirmed":
                        case "emailConfirmed":
                            userEditModel.EmailConfirmed = prop.Value.GetBoolean();
                            existingUser.EmailConfirmed = userEditModel.EmailConfirmed;
                            break;
                        case "LockoutBegin":
                        case "lockoutBegin":
                            userEditModel.LockoutBegin = prop.Value.GetDateTimeOffset();
                            existingUser.LockoutBegin = userEditModel.LockoutBegin;
                            break;
                        case "LockoutEnd":
                        case "lockoutEnd":
                            userEditModel.LockoutEnd = prop.Value.GetDateTimeOffset();
                            existingUser.LockoutEnd = userEditModel.LockoutEnd;
                            break;
                        case "Name":
                        case "name":
                            userEditModel.Name = prop.Value.GetString();
                            existingUser.UserName = userEditModel.Name;
                            break;
                        case "Password":
                        case "password":
                            var password = prop.Value.GetString();
                            if (password.Length == DomainUser.SHA256_LENGTH || password.Length == DomainUser.SHA512_LENGTH)
                                userEditModel.Password = password;
                            else
                                userEditModel.Password = _userManager.PasswordHasher.HashPassword(existingUser, password);

                            existingUser.PasswordHash = userEditModel.Password;
                            break;
                        case "PhoneNumber":
                        case "phoneNumber":
                            userEditModel.PhoneNumber = prop.Value.GetString();
                            existingUser.PhoneNumber = userEditModel.PhoneNumber;
                            break;
                        case "PhoneNumberConfirmed":
                        case "phoneNumberConfirmed":
                            userEditModel.PhoneNumberConfirmed = prop.Value.GetBoolean();
                            existingUser.PhoneNumberConfirmed = userEditModel.PhoneNumberConfirmed;
                            break;
                        case "Properties":
                        case "properties":
                            userEditModel.Properties = JsonSerializer.Deserialize<Dictionary<string, string>>(prop.Value.GetRawText());
                            existingUser.Properties = userEditModel.Properties;
                            break;
                        case "SysUser":
                        case "sysUser":
                            userEditModel.SysUser = prop.Value.GetString();
                            existingUser.SysUser = userEditModel.SysUser;
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            userEditModel.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            existingUser.SysStatus = userEditModel.SysStatus;
                            break;
                        case "TwoFactorEnabled":
                        case "twoFactorEnabled":
                            userEditModel.TwoFactorEnabled = prop.Value.GetBoolean();
                            existingUser.TwoFactorEnabled = userEditModel.TwoFactorEnabled;
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainUserRole).Name} JSON");
                }
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };


            results.Add(await _userManager.UpdateAsync(existingUser));


            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return new ObjectResult(failures) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(userEditModel) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> DeleteAsync(string name) {

            var existingUser = _dbContext.Set<DomainUser>().FirstOrDefault(u => u.UserName == name);

            if (existingUser == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };

            var results = new List<IdentityResult>();

            results.Add(await _userManager.DeleteAsync(existingUser));

            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return new ObjectResult(failures) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };

        }



    }



}