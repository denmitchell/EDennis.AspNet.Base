using System.Collections.Generic;
using System.Linq;

namespace EDennis.NetStandard.Base {
    public class SearchTable<TEntity> : List<SearchRow<TEntity>> 
        where TEntity : class {
        
        public string Where {
            get {
                var list = new List<string>();
                foreach (var row in this.Where(x=>!string.IsNullOrEmpty(x.Expression)))
                    list.Add(row.Expression);

                if (list.Count == 0)
                    return null;

                return string.Join(" and ", list);
            }
        }
    }
}
