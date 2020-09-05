using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace EDennis.AspNetIdentityServer.Areas.Identity.Pages.Account.Admin {
    public partial class IndexModel : DynamicLinqPageModel<SearchableDomainUser> {
        public const int PAGE_SIZE = 10;
        public const string ORDER_BY = "UserName";

        private readonly DomainIdentityDbContext _dbContext;
        public override int SearchTableRowCount => 3;

        public IndexModel(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
            PageSize = PAGE_SIZE;

            Organizations = _dbContext.Organizations
                .ToList()
                .Select(o => new SelectListItem { Text = o.Name, Value = o.Name });

            Applications = _dbContext.Applications
                .ToList()
                .Select(a => new SelectListItem { Text = a.Name, Value = a.Name });
        }


        [BindProperty]
        public IEnumerable<SelectListItem> Organizations { get; set; }


        [BindProperty]
        public IEnumerable<SelectListItem> Applications { get; set; }


        #region query string aliases
        [BindProperty] public ComparisonOperator UsrOp { get; set; }
        [BindProperty] public string Usr { get; set; }
        [BindProperty] public string Org { get; set; }
        [BindProperty] public string App { get; set; }
        #endregion


        public async Task<IActionResult> OnGetAsync(
            ComparisonOperator UsrOp, string Usr, string Org, string App,
            int pageNumber = 1, int? totalRecords = null) {

            Organizations =
                new SelectListItem[] { new SelectListItem { Value = null, Text = "" } }
                .Union(
                    Organizations.Where(o => User.Claims.Any(
                    c => c.Type == DomainClaimTypes.SuperAdmin
                    || (c.Type.StartsWith(DomainClaimTypes.ApplicationRolePrefix) && c.Value.Equals(DomainClaimValues.Admin))
                    || (c.Type == DomainClaimTypes.OrganizationAdminFor && c.Value == o.Value)))
                )
                .ToList();

            Applications =
                new SelectListItem[] { new SelectListItem { Value = null, Text = "" } }
                .Union(
                    Applications.Where(a => User.Claims.Any(
                    c => c.Type == DomainClaimTypes.SuperAdmin
                    || (c.Type == DomainClaimTypes.ApplicationRole(a.Value) && c.Value == DomainClaimValues.Admin)
                    || (c.Type == DomainClaimTypes.OrganizationAdminFor)))
                )
                .ToList();


            SearchTable[0].FieldName = "UserName";
            SearchTable[0].Operator = UsrOp;
            SearchTable[0].FieldValue = Usr;

            SearchTable[1].FieldName = "Organization";
            SearchTable[1].Operator = ComparisonOperator.Equals;
            SearchTable[1].FieldValue = Org;

            SearchTable[2].FieldName = "Applications";
            SearchTable[2].Operator = ComparisonOperator.Contains;
            SearchTable[2].FieldValue = App;


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
