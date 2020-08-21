using EDennis.NetStandard.Base;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EDennis.Samples.ColorApp {
    public class ColorDbContextDesignTimeFactory : SqlServerDbContextDesignTimeFactory<ColorContext>{ }


    public class ColorContext : DbContext  {

        public ColorContext(DbContextOptions<ColorContext> options) : base(options) { }

        public virtual DbSet<Rgb> Rgb { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder
                .ConfigureTemporalEntity<Rgb>(p => p.Id, true, true);
        }

    }
}
