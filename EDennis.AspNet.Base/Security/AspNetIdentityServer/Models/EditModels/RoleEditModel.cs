using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EDennis.AspNet.Base.Security {
    public class RoleEditModel {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string OrganizationName { get; set; }
        public string ApplicationName { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
