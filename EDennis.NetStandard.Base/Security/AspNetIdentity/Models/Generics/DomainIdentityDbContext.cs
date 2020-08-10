using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace EDennis.NetStandard.Base {
    public abstract class DomainIdentityDbContext<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>

        : IdentityDbContext<
            DomainUser<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>,
            DomainRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, 
            Guid,
            DomainUserClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, 
            DomainUserRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, 
            DomainUserLogin<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, 
            DomainRoleClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, 
            DomainUserToken<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>>

        where TUser : DomainUser<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TOrganization : DomainOrganization<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserClaim : DomainUserClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserLogin : DomainUserLogin<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserToken : DomainUserToken<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRole : DomainRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TApplication : DomainApplication<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRoleClaim : DomainRoleClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserRole : DomainUserRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new() {        
        

        public DomainIdentityDbContext(DbContextOptions<DomainIdentityDbContext<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>> options) :
            base(options) { }


        protected override void OnModelCreating(ModelBuilder builder) {
            //base.OnModelCreating(builder);

            builder.Entity<TUser>(e => {
                e.ConfigureTemporalEntity(builder, u => u.Id, false, true, "AspNetUsers");
                
                e.Property(u => u.Id)
                    .ValueGeneratedNever();
                
                e.Property(u => u.UserName)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .IsRequired(true);

                e.Property(u => u.NormalizedUserName)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .IsRequired(true);

                e.Property(u => u.Email)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .IsRequired(false);

                e.Property(u => u.NormalizedEmail)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .IsRequired(false);

                e.Property(u => u.PasswordHash)
                    .IsUnicode(false)
                    .IsRequired(false);

                e.Property(u => u.EmailConfirmed)
                    .IsRequired(true)
                    .HasDefaultValueSql("0");

                e.Property(u => u.SecurityStamp)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsRequired(true);

                e.Property(u => u.ConcurrencyStamp)
                    .HasMaxLength(40)
                    .IsUnicode(false)
                    .IsRequired(true);

                e.Property(u => u.PhoneNumber)
                    .IsUnicode(false)
                    .IsRequired(false);

                e.Property(u => u.PhoneNumberConfirmed)
                    .IsRequired(true)
                    .HasDefaultValueSql("0");

                e.Property(u => u.TwoFactorEnabled)
                    .IsRequired(true)
                    .HasDefaultValueSql("0");

                e.Property(u => u.AccessFailedCount)
                    .IsRequired(true)
                    .HasDefaultValueSql("0");

                e.Property(u => u.LockoutBegin)
                    .IsRequired(false);

                e.Property(u => u.LockoutEnd)
                    .IsRequired(false);

                e.Property(u => u.LockoutEnabled)
                    .IsRequired(true)
                    .HasDefaultValueSql("0");


                e.Property(u => u.OrganizationId)
                    .IsRequired(false);

                e.HasIndex(i => i.NormalizedUserName)
                    .IsUnique(true);

                e.HasOne(u => u.Organization)
                    .WithMany(o=>o.Users)
                    .HasForeignKey(u => u.OrganizationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

            });

            builder.Entity<TOrganization>(e => {
                e.ConfigureTemporalEntity(builder, u => u.Id, false, true, "AspNetOrganizations");
                e.Property(u => u.Id)
                    .ValueGeneratedNever();
                e.HasIndex(i => i.Name)
                    .IsUnique(true);
                e.Property(p => p.Name)
                    .IsUnicode(false)
                    .HasMaxLength(200);
            });

            builder.Entity<TUserClaim>(e => {
                e.ConfigureTemporalEntity(builder, u => u.Id, false, true, "AspNetUserClaims");
                e.HasIndex(i => new { i.UserId, i.ClaimType, i.ClaimValue } )
                    .IsUnique(true);
                e.Property(e => e.ClaimType)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.Property(e => e.ClaimValue)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.HasOne(uc => uc.User)
                    .WithMany(u => u.Claims)
                    .HasForeignKey(uc => uc.UserId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<TUserLogin>(e => {
                e.ConfigureTemporalEntity(builder, u => new { u.LoginProvider, u.ProviderKey }, false, true, "AspNetUserLogins");
                e.Property(e => e.LoginProvider)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.Property(e => e.ProviderKey)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.HasOne(ul => ul.User)
                    .WithMany(u => u.Logins)
                    .HasForeignKey(ul => ul.UserId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<TUserToken>(e => {
                e.ConfigureTemporalEntity(builder, u => new { u.UserId, u.LoginProvider, u.Name }, false, true, "AspNetUserTokens");
                e.Property(e => e.LoginProvider)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.Property(e => e.Name)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.Property(e => e.Value)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.HasOne(ut => ut.User)
                    .WithMany(u => u.Tokens)
                    .HasForeignKey(ut => ut.UserId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<TRole>(e => {
                //remove unique index on NormalizedName;
                e.Metadata.RemoveIndex(new[] { e.Property(r => r.NormalizedName).Metadata });
                e.ConfigureTemporalEntity(builder, u => u.Id, false, true, "AspNetRoles");
                e.HasIndex(i => new { i.ApplicationId, i.Name })
                    .IsUnique(true);
                e.Property(p => p.Name)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.Property(p => p.NormalizedName)
                    .IsRequired(false)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.Property(p => p.ConcurrencyStamp)
                    .IsRequired(false)
                    .IsUnicode(false)
                    .HasMaxLength(40);
                e.HasOne(r => r.Application)
                    .WithMany(a => a.Roles)
                    .HasForeignKey(r => r.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<TApplication>(e => {
                e.ConfigureTemporalEntity(builder, u => u.Id, false, true, "AspNetApplications");
                e.Property(u => u.Id)
                    .ValueGeneratedNever();
                e.HasIndex(i => i.Name)
                    .IsUnique(true);
                e.Property(p => p.Name)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(200);
                e.HasMany(a => a.Roles)
                    .WithOne(r => r.Application)
                    .HasForeignKey(a => a.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            builder.Entity<TRoleClaim>(e => {
                e.ConfigureTemporalEntity(builder, u => u.Id, false, true, "AspNetRoleClaims");
                e.HasIndex(i => new { i.RoleId, i.ClaimType, i.ClaimValue })
                    .IsUnique(true);
                e.Property(e => e.ClaimType)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.Property(e => e.ClaimValue)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                e.HasOne(rc => rc.Role)
                    .WithMany(r => r.Claims)
                    .HasForeignKey(rc => rc.RoleId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<TUserRole>(e => {
                e.ConfigureTemporalEntity(builder, u => new { u.UserId, u.RoleId }, false, true, "AspNetUserRoles");
                e.HasOne(ur=>ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.ClientCascade);
                e.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });


            builder.Entity<DomainUserView>(e => {
                e.ToView("AspNetUsersView");
            });


            builder.Entity<DomainRoleView>(e => {
                e.ToView("AspNetRolesView");
            });


            builder.Entity<UserClientApplicationRole>(e => {
                e.ToView("UserClientApplicationRoles")
                    .HasKey(p => new { p.UserId, p.ClientId, p.ApplicationName, p.RoleName });
            });




        }


    }

}
