using EDennis.AspNet.Base;
using EDennis.AspNet.Base.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq;

namespace Hr.PersonApi.Models {

    public class HrContextDesignTimeFactory : MigrationsExtensionsDbContextDesignTimeFactory<HrContext> { }

    public class HrContext : DbContext {
        public HrContext(DbContextOptions<HrContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.HasSequence<int>("seqPerson", opt => {
                opt.StartsAt(1)
                .IncrementsBy(1);
            });
            modelBuilder.HasSequence<int>("seqAddress", opt => {
                opt.StartsAt(1)
                .IncrementsBy(1);
            });

            modelBuilder.Entity<Person>(e => {
                e.ToTable("Person")
                 .HasKey("Id");

                e.Property("Id")
                 .HasDefaultValueSql("next value for seqPerson");

                e.Property(d => d.SysStatus)
                  .HasColumnType("tinyint");
            });

            modelBuilder.Entity<PersonHistory>(e => {
                e.ToTable("Person", "dbo_history")
                 .HasKey(k=> new { k.Id, k.SysStart });
            });

            modelBuilder.Entity<Address>(e => {
                e.ToTable("Address")
                 .HasKey("Id");

                e.Property("Id")
                 .HasDefaultValueSql("next value for seqAddress");

                e.HasOne(a => a.Person)
                 .WithMany(p => p.Addresses)
                 .HasForeignKey(f => f.PersonId)
                 .OnDelete(DeleteBehavior.ClientCascade);

                e.Property(d => d.SysStatus)
                  .HasColumnType("tinyint");
            });

            modelBuilder.Entity<AddressHistory>(e => {
                e.ToTable("Address", "dbo_history")
                 .HasKey(k => new { k.Id, k.SysStart });
            });

            modelBuilder.Entity<State>(e => {
                e.ToTable("State")
                .HasKey("Code");
            });
        }
    }
}
