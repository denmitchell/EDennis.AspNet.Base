using EDennis.AspNet.Base.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserRepo {

        public DomainIdentityDbContext _dbContext;
        public UserManager<DomainUser> _userManager;

        public DomainUserRepo(DbContextProvider<DomainIdentityDbContext> provider, UserManager<DomainUser> userManager) {
            _dbContext = provider.DbContext;
            _userManager = userManager;
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
                results.Add(await _userManager.AddClaimsAsync(user, userEditModel.Claims.ToClaims()));

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

            var userEditModel = new UserEditModel();

            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Organization":
                        case "organization":
                            userEditModel.Organization = prop.Value.GetString();
                            break;
                        case "Roles":
                        case "roles":
                            userEditModel.Roles = prop.Value.EnumerateArray().Select(x => x.GetString());
                            break;
                        case "Claims":
                        case "claims":
                            userEditModel.Claims = JsonSerializer.Deserialize<Dictionary<string, string[]>>(prop.Value.GetRawText());
                            break;
                        case "AccessFailedCount":
                        case "accessFailedCount":
                            userEditModel.AccessFailedCount = prop.Value.GetInt32();
                            break;
                        case "Email":
                        case "email":
                            userEditModel.Email = prop.Value.GetString();
                            break;
                        case "EmailConfirmed":
                        case "emailConfirmed":
                            userEditModel.EmailConfirmed = prop.Value.GetBoolean();
                            break;
                        case "LockoutBegin":
                        case "lockoutBegin":
                            userEditModel.LockoutBegin = prop.Value.GetDateTimeOffset();
                            break;
                        case "LockoutEnd":
                        case "lockoutEnd":
                            userEditModel.LockoutEnd = prop.Value.GetDateTimeOffset();
                            break;
                        case "Name":
                        case "name":
                            userEditModel.Name = prop.Value.GetString();
                            break;
                    }
                } catch (InvalidOperationException ex) {

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
                                results.Add(await _userManager.AddClaimsAsync(user, userEditModel.Claims.ToClaims()));

                            var failures = results.SelectMany(r => r.Errors);

                            if (failures.Count() > 0)
                                return new ObjectResult(failures) { StatusCode = StatusCodes.Status409Conflict };
                            else
                                return new ObjectResult(userEditModel) { StatusCode = StatusCodes.Status200OK };

                    }





                }

}
