using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class DomainIdentityDbContext 
        : IdentityDbContext<DomainUser, DomainRole, Guid, DomainUserClaim, 
            DomainUserRole, DomainUserLogin, IdentityRoleClaim<Guid>, DomainUserToken> {

        public DbSet<DomainApplication> Applications { get; set; }
        public DbSet<DomainOrganization> Organizations { get; set; }
        public DbSet<UserClientApplicationRoles> UserClientApplicationRoles { get; set; }

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

            builder.Entity<UserClientApplicationRoles>(e => {
                e.ToView("UserClientApplicationRoles")
                .HasKey(p => new { p.UserId, p.ClientId, p.ApplicationName, p.RoleName });
            });

        }


    }
}
