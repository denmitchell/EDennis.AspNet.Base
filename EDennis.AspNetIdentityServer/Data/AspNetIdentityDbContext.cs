using EDennis.AspNetIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EDennis.AspNetIdentityServer.Data {
    public class AspNetIdentityDbContext : IdentityDbContext<IdentityUser> {
        public DbSet<IdentityDomain> Domains { get; set; }
        public AspNetIdentityDbContext(DbContextOptions<AspNetIdentityDbContext> options)
            : base(options) {         
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<IdentityDomain>(e => {
                e.ToTable("AspNetDomains")
                .HasKey(e => e.Id);
            });

            //to improve query performance on claims
            builder.Entity<IdentityUserClaim<int>>(e => {
                e.HasIndex(p => new { p.ClaimValue, p.ClaimType });
            });

            //to improve query performance on claims
            builder.Entity<IdentityRoleClaim<int>>(e => {
                e.HasIndex(p => new { p.ClaimValue, p.ClaimType });
            });

        }
    }
}
