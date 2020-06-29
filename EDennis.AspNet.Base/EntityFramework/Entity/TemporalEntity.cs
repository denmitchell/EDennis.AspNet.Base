using System;
using System.Text.Json;

namespace EDennis.AspNet.Base.Models {
    public abstract class TemporalEntity : CrudEntity {

        public DateTime SysStart { get; set; } = DateTime.Now;
        public DateTime SysEnd { get; set; } = DateTime.MaxValue;
        public static D CloneFrom<S, D>(S source)
              where S : TemporalEntity
              where D : TemporalEntity =>
              JsonSerializer.Deserialize<D>(JsonSerializer.Serialize(source));
    }
}
