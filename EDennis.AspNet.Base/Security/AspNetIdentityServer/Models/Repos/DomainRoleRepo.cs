using EDennis.AspNet.Base.EntityFramework;
using EDennis.AspNet.Base.Security.Extensions;
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
    public class DomainRoleRepo {

        public DomainIdentityDbContext _dbContext;
        public RoleManager<DomainRole> _roleManager;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");


        public DomainRoleRepo(DbContextProvider<DomainIdentityDbContext> provider, RoleManager<DomainRole> roleManager) {
            _dbContext = provider.DbContext;
            _roleManager = roleManager;
        }



        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var role = await FindAsync(pathParameter);
            if (role == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                var roleEditModel = role.ToEditModel();
                return new ObjectResult(roleEditModel) { StatusCode = StatusCodes.Status200OK };
            }
        }





        public async Task<ObjectResult> GetAsync(string appName = null,
            int? pageNumber = 1, int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            var qry = _dbContext.Roles as IQueryable<DomainRole>;

            if (appName != null)
                qry = qry.Where(r => r.Application.Name == appName);

            qry = qry.Skip(skip)
                .Take(take)
                .AsNoTracking();
                
            var result = (await qry.ToListAsync()).Select(x=>x.ToEditModel());

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };

        }



        private async Task<DomainRole> FindAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _roleManager.FindByIdAsync(pathParameter);
            else
                return await _roleManager.FindByNameAsync(pathParameter);
        }


        public async Task<ObjectResult> CreateAsync(RoleEditModel roleEditModel, ModelStateDictionary modelState) {


            var existingRole = _dbContext.Set<DomainRole>().FirstOrDefault(r => r.Name == roleEditModel.Name);

            if (existingRole != null) {
                modelState.AddModelError("Name", $"A role with Name ='{roleEditModel.Name}' already exists.");
            }


            DomainApplication existingApplication;

            if (roleEditModel.Application != null) {
                existingApplication = _dbContext.Set<DomainApplication>().FirstOrDefault(o => o.Name == roleEditModel.Application);

                if (existingApplication == null) {
                    modelState.AddModelError("Application", $"An Application with Name ='{roleEditModel.Application}' does not exist.");
                }
            }


            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };


            var user = new DomainRole {
                Id = Guid.NewGuid(),
                Name = roleEditModel.Name,
                NormalizedName = roleEditModel.Name.ToUpper()
            };

            var results = new List<IdentityResult> {
                await _roleManager.CreateAsync(user)
            };

            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return new ObjectResult(failures) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(roleEditModel) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> PatchAsync(string name, JsonElement jsonElement, ModelStateDictionary modelState) {


            var existingRole = _dbContext.Set<DomainRole>().FirstOrDefault(r => r.Name == name);

            if (existingRole == null) {
                modelState.AddModelError("Name", $"A role with Name ='{name}' does not exist.");
            }

            var results = new List<IdentityResult>();
            var roleEditModel = existingRole.ToEditModel();


            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Application":
                        case "application":
                            roleEditModel.Application = prop.Value.GetString();
                            DomainApplication existingApplication = null;

                            if (roleEditModel.Application != null) {
                                existingApplication = _dbContext.Set<DomainApplication>().FirstOrDefault(o => o.Name == roleEditModel.Application);

                                if (existingApplication == null) {
                                    modelState.AddModelError("Application", $"An application with Name ='{roleEditModel.Application}' does not exists.");
                                }
                            }
                            existingRole.Application = existingApplication;
                            break;
                        case "Name":
                        case "name":
                            roleEditModel.Name = prop.Value.GetString();
                            existingRole.Name = roleEditModel.Name;
                            break;
                        case "Properties":
                        case "properties":
                            roleEditModel.Properties = JsonSerializer.Deserialize<Dictionary<string, string>>(prop.Value.GetRawText());
                            existingRole.Properties = roleEditModel.Properties;
                            break;
                        case "SysUser":
                        case "sysUser":
                            roleEditModel.SysUser = prop.Value.GetString();
                            existingRole.SysUser = roleEditModel.SysUser;
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            roleEditModel.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            existingRole.SysStatus = roleEditModel.SysStatus;
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainRole).Name} JSON");
                }
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };


            results.Add(await _roleManager.UpdateAsync(existingRole));


            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return new ObjectResult(failures) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(roleEditModel) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> DeleteAsync(string name) {

            var existingRole = _dbContext.Set<DomainRole>().FirstOrDefault(r => r.Name == name);

            if (existingRole == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };

            var results = new List<IdentityResult>();

            results.Add(await _roleManager.DeleteAsync(existingRole));

            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return new ObjectResult(failures) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };

        }



    }



}