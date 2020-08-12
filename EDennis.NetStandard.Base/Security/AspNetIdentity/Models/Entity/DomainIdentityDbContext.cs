using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace EDennis.NetStandard.Base {


    public class DomainIdentityDbContext : DomainIdentityDbContext<DomainUser, DomainRole>{

        public DomainIdentityDbContext(DbContextOptions<DomainIdentityDbContext<DomainUser, DomainRole>> options) :
            base(options) { }
    }





    public class DomainIdentityDbContext<TUser, TRole>

        : IdentityDbContext<TUser, TRole, int,
            IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>, 
            IdentityRoleClaim<int>, IdentityUserToken<int>>

        where TUser : DomainUser
        where TRole : DomainRole{        
        

        public DomainIdentityDbContext(DbContextOptions<DomainIdentityDbContext<TUser,TRole>> options) :
            base(options) { }


        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<TUser>(e => {
                e.Property(u => u.LockoutBegin)
                    .IsRequired(false);

                e.Property(u => u.Organization)
                    .IsRequired(false);
            });



            builder.Entity<DomainOrganization>(e => {
                e.ConfigureCrudEntity(builder, u => u.Name, false, "AspNetOrganizations");
                e.Property(p => p.Name)
                    .IsUnicode(false)
                    .HasMaxLength(200);
            });


            builder.Entity<TRole>(e => {
                //replace unique index;
                e.Metadata.RemoveIndex(new[] { e.Property(r => r.NormalizedName).Metadata });
                e.HasIndex(i => new { i.Application, i.Name })
                    .IsUnique(true);
            });


            builder.Entity<DomainApplication>(e => {
                e.ConfigureCrudEntity(builder, u => u.Name, false, "AspNetApplications");
                e.Property(p => p.Name)
                    .IsRequired(true)
                    .IsUnicode(false)
                    .HasMaxLength(200);
            });

            builder.Entity<DomainUserView>(e => {
                e.ToView("AspNetUsersView");
            });


            builder.Entity<DomainApplicationView>(e => {
                e.ToView("AspNetApplicationsView");
            });


            builder.Entity<UserClientApplicationRole>(e => {
                e.ToView("UserClientApplicationRoles")
                    .HasKey(p => new { p.UserId, p.ClientId, p.Application, p.Role });
            });




        }


    }

}
