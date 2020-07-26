using System;

namespace EDennis.AspNet.Base.Security {
    public interface IDomainEntity {
        DateTime SysEnd { get; set; }
        DateTime SysStart { get; set; }
        SysStatus SysStatus { get; set; }
        string SysUser { get; set; }

    }
}