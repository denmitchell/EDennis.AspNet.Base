using Microsoft.AspNet.OData.Query;
using System.Collections.Generic;

namespace EDennis.Samples.ColorApp {


    public class RgbSearchRow : ISearchRow {
        public SearchOperator SearchOperator { get; set; }
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? Red { get; set; }
        public int? Green { get; set; }
        public int? Blue { get; set; }

        public List<string> WhereList {
            get {
                var list = new List<string>();

                if (Id != null)
                    list.Add(Expr.Build(SearchOperator, "Id", Id));
                if (Name != null)
                    list.Add(Expr.Build(SearchOperator, "Name", Name));
                if (Red != null)
                    list.Add(Expr.Build(SearchOperator, "Red", Red));
                if (Green != null)
                    list.Add(Expr.Build(SearchOperator, "Green", Green));
                if (Blue != null)
                    list.Add(Expr.Build(SearchOperator, "Blue", Blue));

                return list;
            }
        }

    }
}
