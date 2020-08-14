using System.Collections.Generic;

namespace EDennis.Samples.ColorApp {
    public interface ISearchRow {
        SearchOperator SearchOperator { get; set; }
        List<string> WhereList { get; }
    }
}