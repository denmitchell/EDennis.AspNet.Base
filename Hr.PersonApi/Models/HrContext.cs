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

            modelBuilder.ConfigureTemporalEntity<Person>()
                .ConfigureTemporalEntity<Address>()
                .ConfigureTemporalEntity<State>();

        }
    }
}
