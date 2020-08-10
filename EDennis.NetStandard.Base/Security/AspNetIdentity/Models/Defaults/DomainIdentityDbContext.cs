using Microsoft.EntityFrameworkCore;

namespace EDennis.NetStandard.Base {
    public class DomainIdentityDbContext : DomainIdentityDbContext<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        public DomainIdentityDbContext(DbContextOptions<DomainIdentityDbContext<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole>> options) : base(options) {
        }
    }
}
