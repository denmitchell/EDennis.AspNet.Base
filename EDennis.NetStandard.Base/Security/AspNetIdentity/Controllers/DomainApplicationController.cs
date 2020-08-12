using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {


    public abstract class DomainApplicationController<TContext, TUser, TRole> : CrudController<TContext, DomainApplication>
        where TContext: DomainIdentityDbContext<TUser, TRole>
        where TUser : DomainUser
        where TRole : DomainRole {        


        public DomainApplicationController(DbContextProvider<TContext> provider, 
            ILogger<QueryController<TContext, DomainApplication>> logger
            ) : base(provider, logger) {
        }


        [NonAction]
        public override IQueryable<DomainApplication> Find(string pathParameter) {
            return _dbContext.Set<DomainApplication>().Where(a => a.Name == pathParameter);
        }


        [HttpGet("view")]
        public async Task<IActionResult> GetViewAsync(int pageNumber, int pageSize) {
            try {
                var result = await _dbContext.Set<DomainApplicationView>()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return Ok(result);
            } catch (Exception ex) {
                ModelState.AddModelError("", $"Could not obtain DomainApplicationViewRecords with pageNumber = {pageNumber} and pageSize = {pageSize}: " + ex.Message);
                return Conflict(ModelState);
            }
        }


        /// <summary>
        /// Implement ON UPDATE CASCADE logic, taking care for
        /// possible updates in letter case only
        /// </summary>
        /// <param name="input">new entity data</param>
        /// <param name="existing">existing entity data</param>
        protected override void DoUpdate(DomainApplication input, DomainApplication existing) {
            var existingName = existing.Name;
            
            if (input.Name == existingName)
                return;
            else if(input.Name.ToUpper() != existingName)
                base.DoCreate(input);
    
            Task.Run(() => {
                var roles = _dbContext.Set<TRole>().Where(x => x.Application == existingName);
                foreach (var role in roles)
                    role.Application = input.Name;
                _dbContext.SaveChangesAsync();
            });

            if (input.Name.ToUpper() != existingName) 
                base.DoDelete(input);
        }

        /// <summary>
        /// Implement ON UPDATE CASCADE logic, taking care for
        /// possible updates in letter case only
        /// </summary>
        /// <param name="element">new entity data</param>
        /// <param name="existing">existing entity data</param>
        protected override void DoPatch(JsonElement element, DomainApplication existing) {
            var input = new DomainApplication();
            input.Patch(element, ModelState);
            DoUpdate(input, existing);
        }

    }
}
