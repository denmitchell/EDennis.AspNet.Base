using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public class RoleEditModel : BaseEditModel {
        public string Organization { get; set; }
        public string Application { get; set; }
        public Dictionary<string,string[]> Claims { get; set; }
    }
}
