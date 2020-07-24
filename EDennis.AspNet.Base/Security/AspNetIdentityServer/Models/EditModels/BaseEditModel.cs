using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public abstract class BaseEditModel {
        public string Name { get; set; }
        public string Properties { get; set; } = null;
        public SysStatus SysStatus { get; set; } = SysStatus.Normal;

        public string SysUser { get; set; } = null;

    }
}
