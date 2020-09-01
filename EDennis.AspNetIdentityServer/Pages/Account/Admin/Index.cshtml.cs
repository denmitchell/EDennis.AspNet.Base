using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace EDennis.AspNetIdentityServer.Areas.Identity.Pages.Account.Admin {
    public partial class IndexModel : DynamicLinqPageModel<SearchableDomainUser>
    {
        public const int PAGE_SIZE = 10;
        public const string ORDER_BY = "UserName";

        private readonly DomainIdentityDbContext _dbContext;
        public override int SearchTableRowCount => 3;

        public IndexModel(DomainIdentityDbContext dbContext)
        {
            _dbContext = dbContext;
            PageSize = PAGE_SIZE;

            Organizations = _dbContext.Organizations
                .ToList()
                .Select(o=> new SelectListItem { Text = o.Name, Value = o.Name });
            
            Applications = _dbContext.Applications
                .ToList()
                .Select(a => new SelectListItem { Text = a.Name, Value = a.Name });
        }


        [BindProperty]
        public IEnumerable<SelectListItem> Organizations { get; set; }


        [BindProperty]
        public IEnumerable<SelectListItem> Applications { get; set; }



        public async Task<IActionResult> OnGetAsync(ComparisonOperator usrop, string usr, string org, string app,
                int pageNumber = 1, int? totalRecords = null) {

            Organizations = Organizations.Where(o => User.Claims.Any(
                c => c.Type == "super_admin"
                || c.Type == "app:role" && c.Value.EndsWith(":admin")
                || c.Type == "organization_admin" && c.Value == o.Value));

            Applications = Applications.Where(a => User.Claims.Any(
                c => c.Type == "super_admin"
                || c.Type == "app:role" && c.Value == $"{a}:admin"
                || c.Type == "organization_admin"));


            SearchTable[0].FieldName = "UserName";
            SearchTable[0].Operator = usrop;
            SearchTable[0].FieldValue = usr;

            SearchTable[1].FieldName = "Organization";
            SearchTable[1].Operator = ComparisonOperator.Equals;
            SearchTable[1].FieldValue = org;

            SearchTable[2].FieldName = "Applications";
            SearchTable[2].Operator = ComparisonOperator.Contains;
            SearchTable[2].FieldValue = app;


            var where = SearchTable.Where;
            var skip = (pageNumber - 1) * PAGE_SIZE;

            await Task.Run(() =>
            {
                _dbContext.Set<SearchableDomainUser>()
                    .GetWithDynamicLinq(out DynamicLinqResult<SearchableDomainUser> result,
                    where: where, orderBy: ORDER_BY, skip: skip, take: PAGE_SIZE, totalRecords: totalRecords);

                Load(result);
            });

            return Page();
        }

    }
}
