using System.Collections.Generic;

namespace Admin.UserApi.Models {
    public class Role {
        public string Id { get; set; }
        public string Name { get; set; }

        public List<RoleClaim> Claims { get; set; }

    }
}
