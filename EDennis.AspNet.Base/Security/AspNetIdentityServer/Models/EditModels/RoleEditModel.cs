using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EDennis.AspNetIdentityServer.Models {
    public class RoleModel {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OrganizationName { get; set; }
        public string ApplicationName { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
