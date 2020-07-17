using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System;
using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public class DomainIdentityDbContext 
        : IdentityDbContext<DomainUser, DomainRole, Guid, DomainUserClaim, 
            DomainUserRole, DomainUserLogin, DomainRoleClaim, DomainUserToken> {

        /*
        public DbSet<DomainUser> Users { get; set; }
        public DbSet<DomainRole> Roles { get; set; }
        public DbSet<DomainRoleClaim> RoleClaims {get; set;}
        public DbSet<DomainUserRole> UserRoles { get; set; }
        public DbSet<DomainUserClaim> UserClaims { get; set; }
        public DbSet<DomainUserLogin> UserLogins { get; set; }
        public DbSet<DomainUserToken> UserTokens { get; set; }
        */

        public DbSet<DomainApplication> Applications { get; set; }
        public DbSet<DomainOrganization> Organizations { get; set; }
        public DbSet<UserClientClaims> UserClientClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);


            builder.Entity<DomainApplication>(e => {
                e.ToTable("AspNetIdentityApplications")
                .HasKey(p => p.Id);

                e.Property(u => u.Id)
                .HasDefaultValue(CombGuid.Create());

                e.Property(p => p.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, typeof(Dictionary<string, string>), null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, null)
                    );

            });

            builder.Entity<DomainOrganization>(e => {

                e.ToTable("AspNetIdentityOrganizations")
                .HasKey(p => p.Id);

                e.Property(u => u.Id)
                .HasDefaultValue(CombGuid.Create());

                e.Property(p => p.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, typeof(Dictionary<string, string>), null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, null)
                    );

            });


            builder.Entity<DomainUser>(e => {
                
                e.Property(u => u.Id)
                .HasDefaultValue(CombGuid.Create());

                e.Property(p => p.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, typeof(Dictionary<string, string>), null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, null)
                    );
            });

            builder.Entity<DomainRole>(e => {
                
                e.Property(u => u.Id)
                .HasDefaultValue(CombGuid.Create());
 
                e.Property(p => p.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, typeof(Dictionary<string, string>), null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, null)
                    );
            });




            builder.Entity<UserClientClaims>(e => {
                e.ToTable("UserClientClaims")
                .HasKey(p => new { p.UserId, p.ClientId });
            });

        }

    }
}
