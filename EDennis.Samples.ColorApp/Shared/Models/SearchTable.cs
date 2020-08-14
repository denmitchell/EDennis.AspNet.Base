using System.Collections.Generic;

namespace EDennis.Samples.ColorApp {
    public class SearchTable<TSearchRow> : List<TSearchRow> 
        where TSearchRow : ISearchRow {
        
        public string Where {
            get {
                var list = new List<string>();
                foreach (var row in this)
                    list.AddRange(row.WhereList);

                return string.Join(" and ", list);
            }
        }
    }
}
