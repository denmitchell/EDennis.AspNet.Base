using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public interface ISearchablePageableModel {
        int CurrentPage { get; set; }
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
        int PageCount { get; set; }
        int PageSize { get; set; }
        int RowCount { get; set; }
        SearchTable SearchTable { get; set; }
    }
}