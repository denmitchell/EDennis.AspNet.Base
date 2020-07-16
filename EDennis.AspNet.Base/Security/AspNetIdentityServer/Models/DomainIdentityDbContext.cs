using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace EDennis.AspNet.Base.Security {
    public class DomainIdentityDbContext<TUser, TRole, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> 
        : IdentityDbContext<TUser, TRole, Guid, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        where TUser : DomainUser
        where TUserClaim : DomainUserClaim
        where TUserLogin : DomainUserLogin
        where TUserRole : DomainUserRole
        where TUserToken : DomainUserToken
        where TRole : DomainRole
        where TRoleClaim : DomainRoleClaim {

        public DbSet<DomainUser> Users { get; set; }
        public DbSet<DomainRole> Roles { get; set; }
        public DbSet<DomainRoleClaim> RoleClaims {get; set;}
        public DbSet<DomainUserRole> UserRoles { get; set; }
        public DbSet<DomainUserClaim> UserClaims { get; set; }
        public DbSet<DomainUserLogin> UserLogins { get; set; }
        public DbSet<DomainUserToken> UserTokens { get; set; }

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
            });

            builder.Entity<DomainOrganization>(e => {
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
