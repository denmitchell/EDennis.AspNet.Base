using System;

namespace EDennis.NetStandard.Base {
    public interface IDomainEntity {
        DateTime SysEnd { get; set; }
        DateTime SysStart { get; set; }
        SysStatus SysStatus { get; set; }
        string SysUser { get; set; }

    }
}