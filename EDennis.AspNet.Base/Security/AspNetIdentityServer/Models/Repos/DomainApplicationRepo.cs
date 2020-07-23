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
    public class DomainAdminRepo {

        public DomainIdentityDbContext _dbContext;

        public static Regex idPattern = new Regex("[0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12}");

        public DomainAdminRepo(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }


        public async Task<ObjectResult> GetAsync(string pathParameter) {
            var application = await FindAsync(pathParameter);
            if (application == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };
            else {
                var applicationEditModel = application.ToEditModel();
                return new ObjectResult(applicationEditModel) { StatusCode = StatusCodes.Status200OK };
            }
        }





        public async Task<ObjectResult> GetAsync(int? pageNumber = 1, 
            int? pageSize = 100) {


            var skip = (pageNumber ?? 1 - 1) * pageSize ?? 100;
            var take = pageSize ?? 100;

            var qry = _dbContext.Roles as IQueryable<DomainRole>;

            qry = qry.Skip(skip)
                .Take(take)
                .AsNoTracking();
                
            var result = (await qry.ToListAsync()).Select(x=>x.ToEditModel());

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };

        }



        private async Task<DomainApplication> FindAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _dbContext.FindAsync<DomainApplication>(Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<DomainApplication>()
                    .FirstOrDefaultAsync(a=>a.Name == pathParameter);
        }


        public async Task<ObjectResult> CreateAsync(ApplicationEditModel applicationEditModel, 
            ModelStateDictionary modelState, string sysUser) {


            var existingApplication = _dbContext.Set<DomainRole>().FirstOrDefault(r => r.Name == applicationEditModel.Name);

            if (existingApplication != null) {
                modelState.AddModelError("Name", $"An application with Name ='{applicationEditModel.Name}' already exists.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }


            var application = new DomainApplication {
                Id = CombGuid.Create(),
                Name = applicationEditModel.Name,
                Properties = applicationEditModel.Properties,
                SysStatus = SysStatus.Normal,
                SysUser = sysUser
            };

            try {
                await _dbContext.AddAsync(application);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot add application '{applicationEditModel.Name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(applicationEditModel) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> PatchAsync(string name, JsonElement jsonElement, 
            ModelStateDictionary modelState, string sysUser) {


            var existingApplication = _dbContext.Set<DomainApplication>().FirstOrDefault(r => r.Name == name);

            if (existingApplication == null) {
                modelState.AddModelError("Name", $"An application with Name ='{name}' does not exist.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status404NotFound };
            }

            var applicationEditModel = existingApplication.ToEditModel();


            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Name":
                        case "name":
                            applicationEditModel.Name = prop.Value.GetString();
                            existingApplication.Name = applicationEditModel.Name;
                            break;
                        case "Properties":
                        case "properties":
                            applicationEditModel.Properties = JsonSerializer.Deserialize<Dictionary<string, string>>(prop.Value.GetRawText());
                            existingApplication.Properties = applicationEditModel.Properties;
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            applicationEditModel.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            existingApplication.SysStatus = applicationEditModel.SysStatus;
                            break;
                    }
                    applicationEditModel.SysUser = sysUser;
                    existingApplication.SysUser = sysUser;

                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainRole).Name} JSON");
                }
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status400BadRequest };


            try {
                _dbContext.Update(existingApplication);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot update application '{name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(applicationEditModel) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> DeleteAsync(string name, ModelStateDictionary modelState, 
            string sysUser) {

            var existingApplication = _dbContext.Set<DomainApplication>().FirstOrDefault(r => r.Name == name);

            if (existingApplication == null)
                return new ObjectResult(null) { StatusCode = StatusCodes.Status404NotFound };

            try {
                _dbContext.Remove(existingApplication);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot delete application '{name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            var failures = results.SelectMany(r => r.Errors);

            if (failures.Count() > 0)
                return new ObjectResult(failures) { StatusCode = StatusCodes.Status409Conflict };
            else
                return new ObjectResult(null) { StatusCode = StatusCodes.Status204NoContent };

        }



    }



}