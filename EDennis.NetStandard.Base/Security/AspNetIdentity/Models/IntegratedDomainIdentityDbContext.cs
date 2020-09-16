using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class IntegratedDomainIdentityDbContext : DomainIdentityDbContext, IPersistedGrantDbContext {

        private readonly IOptions<OperationalStoreOptions> _operationalStoreOptions;

        public IntegratedDomainIdentityDbContext(DbContextOptions<IntegratedDomainIdentityDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options) {
            _operationalStoreOptions = operationalStoreOptions;
        }

        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
        public async Task<int> SaveChangesAsync() {
            return await base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.ConfigurePersistedGrantContext(_operationalStoreOptions.Value);
        }

    }
}
