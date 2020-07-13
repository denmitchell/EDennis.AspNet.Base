using Microsoft.AspNetCore.Identity;
using System;

namespace EDennis.AspNet.Base.Security {
    public class DomainUser : IdentityUser<Guid> {
        public Guid OrganizationId { get; set; }
    }
}
