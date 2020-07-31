using System.Collections.Generic;

namespace EDennis.NetStandard.Base
{
    public class DeserializableLoadResult<T> {
        public ICollection<T> data { get; set; }
        public int totalCount { get; set; }
        public int groupCount { get; set; }
        public object[] summary { get; set; }
    }
}
