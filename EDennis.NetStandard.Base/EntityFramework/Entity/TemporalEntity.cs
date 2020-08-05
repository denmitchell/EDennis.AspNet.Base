using System;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public abstract class TemporalEntity : CrudEntity, ITemporalEntity {

        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; } = DateTime.Now;
        public DateTime SysEnd { get; set; } = DateTime.MaxValue;

    }
}
