using System.Collections.Generic;
using System.Security.Claims;

namespace EDennis.AspNetIdentityServer.Models {
    public class UserEditModel {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
