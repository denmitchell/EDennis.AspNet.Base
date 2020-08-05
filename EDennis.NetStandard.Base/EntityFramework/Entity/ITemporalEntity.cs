using System;

namespace EDennis.NetStandard.Base {
    public interface ITemporalEntity : ICrudEntity {
        SysStatus SysStatus { get; set; }
        DateTime SysEnd { get; set; }
        DateTime SysStart { get; set; }
    }
}