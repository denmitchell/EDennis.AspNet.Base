using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.AspNet.Base {

    /// <summary>
    /// The result returned from a Dynamic Linq query.  This
    /// version of DynamicLinqResult provides a dynamic
    /// response.  The class is used when the Select property
    /// IS provided (and hence only a subset of properties are 
    /// returned).
    /// </summary>
    public class DynamicLinqResult : DynamicLinqResult<dynamic> {
        public override List<dynamic> Data { get; set; }
    }


    /// <summary>
    /// The result returned from a Dynamic Linq query.  This
    /// generic version of DynamicLinqResult provides a typed
    /// response.  The class is used when the Select property
    /// is not provided (and hence all properties are returned).
    /// </summary>
    /// <typeparam name="TEntity">The relevant model class</typeparam>
    public class DynamicLinqResult<TEntity> {
        public virtual List<TEntity> Data { get; set; }
        public int CurrentPage { get; set; }
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
    }
}
