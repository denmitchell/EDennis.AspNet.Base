using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public class DomainIdentityDbContext 
        : IdentityDbContext<DomainUser, DomainRole, Guid, DomainUserClaim, 
            DomainUserRole, DomainUserLogin, IdentityRoleClaim<Guid>, DomainUserToken> {

        public DbSet<DomainApplication> Applications { get; set; }
        public DbSet<DomainOrganization> Organizations { get; set; }
        public DbSet<UserClientApplicationRole> UserClientApplicationRoles { get; set; }
        public DbSet<ExpandedDomainUser> ExpandedUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);



            builder.Entity<DomainUser>(e => {
                e.Property(u => u.Id)
                    .HasDefaultValue(CombGuid.Create());
            });

            builder.Entity<DomainRole>(e => {
                e.Property(u => u.Id)
                    .HasDefaultValue(CombGuid.Create());
            });

            builder.Entity<DomainApplication>(e => {
                e.ToTable("AspNetIdentityApplications")
                    .HasKey(p => p.Id);

                e.Property(u => u.Id)
                    .HasDefaultValue(CombGuid.Create());
            });

            builder.Entity<DomainOrganization>(e => {
                e.ToTable("AspNetIdentityOrganizations")
                    .HasKey(p => p.Id);

                e.Property(u => u.Id)
                    .HasDefaultValue(CombGuid.Create());
            });


            builder.Entity<ExpandedDomainUser>(e => {
                e.ToView("AspNetIdentityExpandedUsers")
                    .HasKey(p => p.Id);

                e.Property(p => p.ClaimsDictionary)
                    .HasConversion(new SerializerConverter<Dictionary<string, List<string>>>());

                e.Property(p => p.RolesDictionary)
                    .HasConversion(new SerializerConverter<Dictionary<string, List<string>>>());
            });


            builder.Entity<UserClientApplicationRole>(e => {
                e.ToView("UserClientApplicationRoles")
                    .HasKey(p => new { p.UserId, p.ClientId, p.ApplicationName, p.RoleName });
            });




        }


    }
}
