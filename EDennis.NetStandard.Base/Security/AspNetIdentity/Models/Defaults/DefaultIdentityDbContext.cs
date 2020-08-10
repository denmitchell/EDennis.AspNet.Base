using Microsoft.EntityFrameworkCore;

namespace EDennis.NetStandard.Base.Security.AspNetIdentity.Models.Defaults {
    public class DefaultIdentityDbContext : TeDbContext<DefaultUser, DefaultOrganization, DefaultUserClaim, DefaultUserLogin, DefaultUserToken, DefaultRole, DefaultApplication, DefaultRoleClaim, DefaultUserRole> {
        public DefaultIdentityDbContext(DbContextOptions<TeDbContext<DefaultUser, DefaultOrganization, DefaultUserClaim, DefaultUserLogin, DefaultUserToken, DefaultRole, DefaultApplication, DefaultRoleClaim, DefaultUserRole>> options) : base(options) {
        }
    }
}
