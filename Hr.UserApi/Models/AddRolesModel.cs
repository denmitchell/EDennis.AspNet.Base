using System.Collections.Generic;

namespace Hr.UserApi.Models {
    public class EditRolesModel {
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
