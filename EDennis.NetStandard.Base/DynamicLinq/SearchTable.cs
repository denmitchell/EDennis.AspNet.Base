using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class SearchTable : List<SearchRow> {
        
        public string Where {
            get {
                var list = new List<string>();
                foreach (var row in this)
                    list.Add(row.Expression);

                return string.Join(" and ", list);
            }
        }
    }
}
