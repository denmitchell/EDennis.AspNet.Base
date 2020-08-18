using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetStandard.Base {
    public abstract class DynamicLinqPageModel<TEntity> : PageModel, ISearchablePageableModel <TEntity>
        where TEntity : class {


        public DynamicLinqPageModel(){
            SearchTable = new SearchTable<TEntity>();
            for (int i = 0; i < SearchTableRowCount; i++)
                SearchTable.Add(new SearchRow<TEntity>());
        }


        public abstract int SearchTableRowCount { get; }

        public virtual List<TEntity> Data { get; set; } = new List<TEntity>();
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
