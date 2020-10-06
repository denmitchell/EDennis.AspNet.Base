using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;

namespace EDennis.NetStandard.Base {
    public class DbBackedChildClaimCache : IChildClaimCache {

        private readonly IEnumerable<ChildClaim> _childClaims;

        public DbBackedChildClaimCache(DomainIdentityDbContext dbContext, IHostEnvironment env) {
            _childClaims = dbContext.ChildClaims.Where(c => c.ParentType.EndsWith(env.ApplicationName));
        }

        public IEnumerable<ChildClaim> ChildClaims => _childClaims;
    }
}
