using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EDennis.AspNet.Base.Security {
    public class DomainIdentityDbContext<TUser,TRole> : IdentityDbContext<TUser,TRole,string> 
        where TUser : DomainUser
        where TRole : DomainRole {

        public DbSet<IdentityApplication> Applications { get; set; }
        public DbSet<IdentityOrganization> Organizations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.HasSequence<int>("seqAspNetIdentityApplication")
                .StartsAt(1)
                .IncrementsBy(1);

            builder.HasSequence<int>("seqAspNetIdentityOrganization")
                .StartsAt(1)
                .IncrementsBy(1);

            builder.Entity<IdentityApplication>(e => {
                e.ToTable("AspNetIdentityApplication")
                .HasKey("Id");
            });

            builder.Entity<IdentityOrganization>(e => {
                e.ToTable("AspNetIdentityOrganization")
                .HasKey("Id");
            });

        }

    }
}
