using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System;
using System.Linq;
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

                e.HasOne(f => f.Organization)
                    .WithMany(r => r.Users)
                    .HasForeignKey(f => f.OrganizationId)
                    .HasConstraintName("fk_DomainUser_DomainOrganization")
                    .OnDelete(DeleteBehavior.ClientSetNull);

            });

            builder.Entity<DomainRole>(e => {
                
                e.Property(u => u.Id)
                    .HasDefaultValue(CombGuid.Create());
 
                e.Property(p => p.Properties)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, typeof(Dictionary<string, string>), null),
                        v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, null)
                        );

                e.HasOne(f => f.Organization)
                    .WithMany(r => r.Roles)
                    .HasForeignKey(f => f.OrganizationId)
                    .HasConstraintName("fk_DomainRole_DomainOrganization")
                    .OnDelete(DeleteBehavior.ClientSetNull);

                e.HasOne(f => f.Application)
                    .WithMany(r => r.Roles)
                    .HasForeignKey(f => f.ApplicationId)
                    .HasConstraintName("fk_DomainRole_DomainApplication")
                    .OnDelete(DeleteBehavior.ClientSetNull);

            });


            builder.Entity<DomainUserClaim>(e => {
                e.HasOne(f => f.User)
                    .WithMany(r => r.UserClaims)
                    .HasForeignKey(f => f.UserId)
                    .HasConstraintName("fk_DomainUserClaim_DomainUser")
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<DomainUserLogin>(e => {
                e.HasOne(f => f.User)
                    .WithMany(r => r.UserLogins)
                    .HasForeignKey(f => f.UserId)
                    .HasConstraintName("fk_DomainUserLogin_DomainUser")
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<DomainUserRole>(e => {
                e.HasOne(f => f.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(f => f.UserId)
                    .HasConstraintName("fk_DomainUserRole_DomainUser")
                    .OnDelete(DeleteBehavior.ClientCascade);

                e.HasOne(f => f.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(f => f.RoleId)
                    .HasConstraintName("fk_DomainUserRole_DomainRole")
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<DomainUserToken>(e => {
                e.HasOne(f => f.User)
                    .WithMany(r => r.UserTokens)
                    .HasForeignKey(f => f.UserId)
                    .HasConstraintName("fk_DomainUserToken_DomainUser")
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<DomainRoleClaim>(e => {
                e.HasOne(f => f.Role)
                    .WithMany(r => r.RoleClaims)
                    .HasForeignKey(f => f.RoleId)
                    .HasConstraintName("fk_DomainRoleClaim_DomainRole")
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<UserClientClaims>(e => {
                e.ToTable("UserClientClaims")
                .HasKey(p => new { p.UserId, p.ClientId });
            });

        }

    }
}
