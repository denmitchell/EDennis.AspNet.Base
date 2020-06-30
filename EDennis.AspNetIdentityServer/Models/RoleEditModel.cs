using System.Collections.Generic;
using System.Security.Claims;

namespace EDennis.AspNetIdentityServer.Models {
    public class RoleEditModel {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
