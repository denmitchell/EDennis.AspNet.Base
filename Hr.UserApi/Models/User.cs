using System.Collections.Generic;

namespace Admin.UserApi.Models {
    public class User {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        public List<UserClaim> Claims { get; set; }
        public List<UserRole> Roles { get; set; }

    }
}
