using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public interface ISearchablePageableModel<TEntity>
        where TEntity : class {
        int CurrentPage { get; set; }
        bool HasNextPage { get; }
        bool HasPreviousPage { get; }
        int PageCount { get; set; }
        int PageSize { get; set; }
        int RowCount { get; set; }
        SearchTable<TEntity> SearchTable { get; set; }
    }
}