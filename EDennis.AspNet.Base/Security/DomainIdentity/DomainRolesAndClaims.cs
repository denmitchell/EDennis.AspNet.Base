using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public class DomainRolesAndClaims {
        public IEnumerable<DomainRole> DomainRoles { get; set; }
        public IEnumerable<IdentityRoleClaim<Guid>> AspNetRoleClaims { get; set; }
    }
}
