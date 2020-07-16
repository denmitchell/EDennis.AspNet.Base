using System;
using System.Text.Json;

namespace EDennis.AspNet.Base {
    public interface ITemporalEntity : ICrudEntity {
        DateTime SysEnd { get; set; }
        DateTime SysStart { get; set; }

        public static D CloneFrom<S, D>(S source)
              where S : ITemporalEntity
              where D : ITemporalEntity =>
              JsonSerializer.Deserialize<D>(JsonSerializer.Serialize(source));

    }
}