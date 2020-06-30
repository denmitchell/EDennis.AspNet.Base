using Hr.App.Data.Models;
using Hr.App.Data.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Hr.App.Components {


    public partial class PersonSearchBase : ComponentBase {

        [Inject] public IPersonService PersonService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public int PersonId { get; set; }
        public string IdSearchType { get; set; }
        public string FirstNameSearchType { get; set; }
        public string LastNameSearchType { get; set; }

        protected Pager Pager { get; set; }

        protected string Where {
            get {
                List<string> whereList = new List<string>();
                var p = PersonSearchParameters;
                if (p.Id == null
                    && string.IsNullOrEmpty(p.FirstNamePattern)
                    && string.IsNullOrEmpty(p.LastNamePattern))
                    return null;
                if (p.Id != null)
                    whereList.Add($"Id eq {p.Id}");
                if (!string.IsNullOrEmpty(p.LastNamePattern)) {
                    if (LastNameSearchType == "Equals")
                        whereList.Add($"LastName.Equals(\"{p.LastNamePattern}\", StringComparison.OrdinalIgnoreCase)");
                    else if (LastNameSearchType == "Contains")
                        whereList.Add($"LastName.Contains(\"{p.LastNamePattern}\", StringComparison.OrdinalIgnoreCase)");
                    else
                        whereList.Add($"LastName.StartsWith(\"{p.LastNamePattern}\", StringComparison.OrdinalIgnoreCase)");
                }
                if (!string.IsNullOrEmpty(p.FirstNamePattern)) {
                    if (FirstNameSearchType == "Equals")
                        whereList.Add($"FirstName.Equals(\"{p.FirstNamePattern}\", StringComparison.OrdinalIgnoreCase)");
                    else if (FirstNameSearchType == "Contains")
                        whereList.Add($"FirstName.Contains(\"{p.FirstNamePattern}\", StringComparison.OrdinalIgnoreCase)");
                    else
                        whereList.Add($"FirstName.StartsWith(\"{p.FirstNamePattern}\", StringComparison.OrdinalIgnoreCase)");
                }

                return string.Join(" and ", whereList);
            }
        }

        public PersonSearchParameters PersonSearchParameters { get; set; } = new PersonSearchParameters();


        public IEnumerable<Person> Persons { get; set; } = new List<Person>();

        protected int? rowCount;


        //protected async override Task OnInitializedAsync() => await ExecuteSearchAsync(true);

        public async Task OnPagerChangedAsync(bool _) => await ExecuteSearchAsync(false);



        public async Task OnSearchAsync() => await ExecuteSearchAsync(true);


        private async Task ExecuteSearchAsync(bool resetRowCount) {
            PagedResult pagedResult;
            if (resetRowCount) {
                pagedResult = await PersonService.GetPageAsync(Where, "Id", 1, Pager.PageSize, null);
            } else {
                pagedResult = await PersonService.GetPageAsync(Where, "Id", Pager.CurrentPage, Pager.PageSize, rowCount);
            }
            Persons = pagedResult.Queryable.Cast<Person>();
            rowCount = pagedResult.RowCount;
        }

        protected void OnNewAsync(bool _) {
            NavigationManager.NavigateTo("/PersonDetail/0?Editable=true");
        }

    }
}
