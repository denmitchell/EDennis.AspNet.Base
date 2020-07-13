using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace EDennis.AspNet.Base.Security {
    public class DomainIdentityDbContext<TUser,TRole> : IdentityDbContext<TUser,TRole,Guid> 
        where TUser : DomainUser
        where TRole : DomainRole {

        public DbSet<IdentityApplication> Applications { get; set; }
        public DbSet<IdentityOrganization> Organizations { get; set; }
        public DbSet<UserClientClaims> UserClientClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);


            builder.Entity<IdentityApplication>(e => {
                e.ToTable("AspNetIdentityApplications")
                .HasKey(p => p.Id);

                e.Property(u => u.Id)
                .HasDefaultValue(CombGuid.Create());
            });

            builder.Entity<IdentityOrganization>(e => {
                e.ToTable("AspNetIdentityOrganizations")
                .HasKey(p => p.Id);

                e.Property(u => u.Id)
                .HasDefaultValue(CombGuid.Create());
            });

            builder.Entity<DomainUser>(b => {
                b.Property(u => u.Id)
                .HasDefaultValue(CombGuid.Create());
            });

            builder.Entity<DomainRole>(b => {
                b.Property(u => u.Id)
                .HasDefaultValue(CombGuid.Create());
            });

            builder.Entity<UserClientClaims>(e => {
                e.ToTable("UserClientClaims")
                .HasKey(p => new { p.UserId, p.ClientId });
            });

        }

    }
}
