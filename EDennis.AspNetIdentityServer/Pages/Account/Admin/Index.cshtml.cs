using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNetIdentityServer.Areas.Identity.Pages.Account.Admin {
    public partial class IndexModel : PageModel //DynamicLinqPageModel<DomainUser>
    {
        private readonly DomainIdentityDbContext _dbContext;

        public IndexModel(DomainIdentityDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public IEnumerable<SearchResultModel> SearchResults { get; set; }

        public class SearchResultModel
        {
            public int Id { get; set; }
            public string UserName { get; set; }
        }


        public async Task<IActionResult> OnGetAsync()
        {
            SearchResults = await _dbContext.Users.Select(x => new SearchResultModel { Id = x.Id, UserName = x.UserName })
                 .ToListAsync();

            return Page();
        }

    }
}
