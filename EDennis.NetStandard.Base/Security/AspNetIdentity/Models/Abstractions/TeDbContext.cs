using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace EDennis.NetStandard.Base {
    public abstract class TeDbContext<U, O, UC, UL, UT, R, A, RC, UR>

        : IdentityDbContext<
            TeUser<U, O, UC, UL, UT, R, A, RC, UR>,
            TeRole<U, O, UC, UL, UT, R, A, RC, UR>, 
            Guid,
            TeUserClaim<U, O, UC, UL, UT, R, A, RC, UR>, 
            TeUserRole<U, O, UC, UL, UT, R, A, RC, UR>, 
            TeUserLogin<U, O, UC, UL, UT, R, A, RC, UR>, 
            TeRoleClaim<U, O, UC, UL, UT, R, A, RC, UR>, 
            TeUserToken<U, O, UC, UL, UT, R, A, RC, UR>>

        where U : TeUser<U, O, UC, UL, UT, R, A, RC, UR>
        where O : TeOrganization<U, O, UC, UL, UT, R, A, RC, UR>
        where UC : TeUserClaim<U, O, UC, UL, UT, R, A, RC, UR>
        where UL : TeUserLogin<U, O, UC, UL, UT, R, A, RC, UR>
        where UT : TeUserToken<U, O, UC, UL, UT, R, A, RC, UR>
        where R : TeRole<U, O, UC, UL, UT, R, A, RC, UR>
        where A : TeApplication<U, O, UC, UL, UT, R, A, RC, UR>
        where RC : TeRoleClaim<U, O, UC, UL, UT, R, A, RC, UR>
        where UR : TeUserRole<U, O, UC, UL, UT, R, A, RC, UR> {        
        

        public TeDbContext(DbContextOptions<TeDbContext<U, O, UC, UL, UT, R, A, RC, UR>> options) :
            base(options) { }


        public DbSet<TeApplication<U, O, UC, UL, UT, R, A, RC, UR>> Applications { get; set; }
        public DbSet<TeOrganization<U, O, UC, UL, UT, R, A, RC, UR>> Organizations { get; set; }
        public DbSet<UserClientApplicationRole> UserClientApplicationRoles { get; set; }
        public DbSet<ExpandedDomainUser> ExpandedUsers { get; set; }
        public DbSet<ExpandedDomainRole> ExpandedRoles { get; set; }


        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);


            builder.Entity<TeUser<U, O, UC, UL, UT, R, A, RC, UR>>(e => {

                e.ConfigureTemporalEntity(builder, u => u.Id, false, true);

                e.Property(u => u.Id)
                    .ValueGeneratedNever();

                e.HasIndex(i => i.UserName)
                    .IsUnique(true);
            });


            builder.Entity<TeRole<U, O, UC, UL, UT, R, A, RC, UR>>(e => {

                e.ConfigureTemporalEntity(builder, u => u.Id, false, true);

                e.HasIndex(i => new { i.ApplicationId, i.Name })
                    .IsUnique(true);

                e.Property(p => p.Name)
                    .IsUnicode(false)
                    .HasMaxLength(256);
                    
            });


            builder.Entity<IdentityApplication>(e => {
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
