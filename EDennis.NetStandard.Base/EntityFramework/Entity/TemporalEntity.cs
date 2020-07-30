using System;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public abstract class TemporalEntity : CrudEntity, ITemporalEntity {

        public DateTime SysStart { get; set; } = DateTime.Now;
        public DateTime SysEnd { get; set; } = DateTime.MaxValue;

    }
}
