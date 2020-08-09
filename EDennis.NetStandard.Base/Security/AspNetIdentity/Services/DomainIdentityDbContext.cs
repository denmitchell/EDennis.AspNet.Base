using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public class DomainIdentityDbContext 
        : IdentityDbContext<DomainUser, DomainRole, Guid, DomainUserClaim, 
            DomainUserRole, DomainUserLogin, IdentityRoleClaim<Guid>, DomainUserToken> {


        public DomainIdentityDbContext(DbContextOptions<DomainIdentityDbContext> options) :
            base(options) { }


        public DbSet<DomainApplication> Applications { get; set; }
        public DbSet<DomainOrganization> Organizations { get; set; }
        public DbSet<UserClientApplicationRole> UserClientApplicationRoles { get; set; }
        public DbSet<ExpandedDomainUser> ExpandedUsers { get; set; }
        public DbSet<ExpandedDomainRole> ExpandedRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);



            builder.Entity<DomainUser>(e => {
                e.HasAnnotation("SystemVersioned", true);

                e.Property(u => u.Id)
                    .ValueGeneratedNever();

                e.Property(u => u.SysStart)
                    .ValueGeneratedOnAddOrUpdate();

                e.Property(u => u.SysEnd)
                    .ValueGeneratedOnAddOrUpdate();

                e.HasIndex(i => i.UserName)
                    .IsUnique(true);

                e.Property(p => p.SysUser)
                    .IsUnicode(false)
                    .HasMaxLength(150);
            });

            builder.Entity<DomainRole>(e => {
                e.HasAnnotation("SystemVersioned", true);

                e.Property(u => u.Id)
                    .ValueGeneratedNever();

                e.Property(u => u.SysStart)
                    .ValueGeneratedOnAddOrUpdate();

                e.Property(u => u.SysEnd)
                    .ValueGeneratedOnAddOrUpdate();

                e.HasIndex(i => new { i.ApplicationId, i.Name })
                    .IsUnique(true);

                e.Property(p => p.Name)
                    .IsUnicode(false)
                    .HasMaxLength(256);

                e.Property(p => p.SysUser)
                    .IsUnicode(false)
                    .HasMaxLength(150);
                    
            });

            builder.Entity<DomainApplication>(e => {
                e.HasAnnotation("SystemVersioned", true);

                e.ToTable("AspNetApplications")
                    .HasKey(p => p.Id);

                e.Property(u => u.Id)
                    .ValueGeneratedNever();

                e.Property(u => u.SysStart)
                    .ValueGeneratedOnAddOrUpdate();

                e.Property(u => u.SysEnd)
                    .ValueGeneratedOnAddOrUpdate();

                e.HasIndex(i => i.Name)
                    .IsUnique(true);

                e.Property(p => p.Name)
                    .IsUnicode(false)
                    .HasMaxLength(200);

                e.Property(p => p.SysUser)
                    .IsUnicode(false)
                    .HasMaxLength(150);
            });

            builder.Entity<DomainOrganization>(e => {
                e.HasAnnotation("SystemVersioned", true);

                e.ToTable("AspNetOrganizations")
                    .HasKey(p => p.Id);

                e.Property(u => u.Id)
                    .ValueGeneratedNever();

                e.Property(u => u.SysStart)
                    .ValueGeneratedOnAddOrUpdate();

                e.Property(u => u.SysEnd)
                    .ValueGeneratedOnAddOrUpdate();

                e.HasIndex(i => i.Name)
                    .IsUnique(true);

                e.Property(p => p.Name)
                    .IsUnicode(false)
                    .HasMaxLength(200);

                e.Property(p => p.SysUser)
                    .IsUnicode(false)
                    .HasMaxLength(150);
            });


            builder.Entity<ExpandedDomainUser>(e => {
                e.ToView("AspNetExpandedUsers");
            });


            builder.Entity<ExpandedDomainRole>(e => {
                e.ToView("AspNetExpandedRoles");
            });


            builder.Entity<UserClientApplicationRole>(e => {
                e.ToView("UserClientApplicationRoles")
                    .HasKey(p => new { p.UserId, p.ClientId, p.ApplicationName, p.RoleName });
            });




        }


    }
}
