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
                var organizationEditModel = organization.ToEditModel();
                return new ObjectResult(organizationEditModel) { StatusCode = StatusCodes.Status200OK };
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
                
            var result = (await qry.ToListAsync()).Select(x=>x.ToEditModel());

            return new ObjectResult(result) { StatusCode = StatusCodes.Status200OK };

        }



        private async Task<DomainOrganization> FindAsync(string pathParameter) {
            if (idPattern.IsMatch(pathParameter))
                return await _dbContext.FindAsync<DomainOrganization>(Guid.Parse(pathParameter));
            else
                return await _dbContext.Set<DomainOrganization>()
                    .FirstOrDefaultAsync(a=>a.Name == pathParameter);
        }


        public async Task<ObjectResult> CreateAsync(OrganizationEditModel organizationEditModel, 
            ModelStateDictionary modelState, string sysUser) {


            var existingOrganization = _dbContext.Set<DomainOrganization>().FirstOrDefault(r => r.Name == organizationEditModel.Name);

            if (existingOrganization != null) {
                modelState.AddModelError("Name", $"An organization with Name ='{organizationEditModel.Name}' already exists.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }


            var organization = new DomainOrganization {
                Id = CombGuid.Create(),
                Name = organizationEditModel.Name,
                Properties = organizationEditModel.Properties,
                SysStatus = SysStatus.Normal,
                SysUser = sysUser
            };

            try {
                await _dbContext.AddAsync(organization);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot add organization '{organizationEditModel.Name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(organizationEditModel) { StatusCode = StatusCodes.Status200OK };

        }



        public async Task<ObjectResult> PatchAsync(string name, JsonElement jsonElement, 
            ModelStateDictionary modelState, string sysUser) {


            var existingOrganization = _dbContext.Set<DomainOrganization>().FirstOrDefault(r => r.Name == name);

            if (existingOrganization == null) {
                modelState.AddModelError("Name", $"An organization with Name ='{name}' does not exist.");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status404NotFound };
            }

            var organizationEditModel = existingOrganization.ToEditModel();


            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Name":
                        case "name":
                            organizationEditModel.Name = prop.Value.GetString();
                            existingOrganization.Name = organizationEditModel.Name;
                            break;
                        case "Properties":
                        case "properties":
                            organizationEditModel.Properties = JsonSerializer.Deserialize<Dictionary<string, string>>(prop.Value.GetRawText());
                            existingOrganization.Properties = organizationEditModel.Properties;
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            organizationEditModel.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            existingOrganization.SysStatus = organizationEditModel.SysStatus;
                            break;
                    }
                    organizationEditModel.SysUser = sysUser;
                    existingOrganization.SysUser = sysUser;

                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainOrganization).Name} JSON");
                }
            }

            if (modelState.ErrorCount > 0)
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status400BadRequest };


            try {
                _dbContext.Update(existingOrganization);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                modelState.AddModelError("", $"Cannot update organization '{name}': {ex.Message}");
                return new ObjectResult(modelState) { StatusCode = StatusCodes.Status409Conflict };
            }

            return new ObjectResult(organizationEditModel) { StatusCode = StatusCodes.Status200OK };

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