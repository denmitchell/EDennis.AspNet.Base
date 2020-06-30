using System.Collections.Generic;

namespace Hr.UserApi.Models {
    public class User {

        public string Id { get; set; }
        public string Email { get; set; }
        public List<UserClaim> Claims { get; set; }
        public List<UserRole> Roles { get; set; }

    }
}
