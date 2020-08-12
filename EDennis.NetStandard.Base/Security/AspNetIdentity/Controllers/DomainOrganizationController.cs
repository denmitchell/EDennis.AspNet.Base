using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {


    public abstract class DomainOrganizationController<TContext, TUser, TRole> : CrudController<TContext, DomainOrganization>
        where TContext : DomainIdentityDbContext<TUser, TRole>
        where TUser : DomainUser
        where TRole : DomainRole {


        public DomainOrganizationController(DbContextProvider<TContext> provider,
            ILogger<QueryController<TContext, DomainOrganization>> logger
            ) : base(provider, logger) {
        }


        [NonAction]
        public override IQueryable<DomainOrganization> Find(string pathParameter) {
            return _dbContext.Set<DomainOrganization>().Where(a => a.Name == pathParameter);
        }


        /// <summary>
        /// Implement ON UPDATE CASCADE logic, taking care for
        /// possible updates in letter case only
        /// </summary>
        /// <param name="input">new entity data</param>
        /// <param name="existing">existing entity data</param>
        protected override void DoUpdate(DomainOrganization input, DomainOrganization existing) {
            var existingName = existing.Name;

            if (input.Name == existingName)
                return;
            else if (input.Name.ToUpper() != existingName)
                base.DoCreate(input);

            Task.Run(() => {
                var users = _dbContext.Set<TUser>().Where(x => x.Organization == existingName);
                foreach (var user in users)
                    user.Organization = input.Name;
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
        protected override void DoPatch(JsonElement element, DomainOrganization existing) {
            var input = new DomainOrganization();
            input.Patch(element, ModelState);
            DoUpdate(input, existing);
        }

    }
}
