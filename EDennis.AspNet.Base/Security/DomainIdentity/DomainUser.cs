using Microsoft.AspNetCore.Identity;

namespace EDennis.AspNet.Base.Security {
    public class DomainUser : IdentityUser {
        public int? OrganizationId { get; set; }
    }
}
