using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class DynamicLinqComponentBase<TEntity> : ComponentBase where TEntity : class {

        public DynamicLinqComponentBase() {
            SearchTable = new SearchTable<TEntity>();
            for (int i = 0; i < SearchTableRowCount; i++)
                SearchTable.Add(new SearchRow<TEntity>());
        }

        public List<TEntity> Data { get; set; }
        public int SearchTableRowCount { get; } = 2;

        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; } = -1;
        public SearchTable<TEntity> SearchTable { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage != default && CurrentPage < PageCount;

        public void Load(DynamicLinqResult<TEntity> result) {
            Data = result.Data;
            CurrentPage = result.CurrentPage;
            PageCount = result.PageCount;
            PageSize = result.PageSize;
            RowCount = result.RowCount;
        }

    }
}