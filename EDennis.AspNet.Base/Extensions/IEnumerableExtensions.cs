using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace EDennis.AspNet.Base.Extensions {
    public static class IEnumerableExtensions {
        public static ICustomQueryParameter ToStringTableTypeParameter(this IEnumerable<string> values) {

            var table = new DataTable();
            table.Columns.Add("Value", typeof(string));

            foreach (var value in values) {
                var row = table.NewRow();
                row["Value"] = value;
            }

            return table.AsTableValuedParameter(typeName: "dbo.StringTableType");
        }
    }
}
