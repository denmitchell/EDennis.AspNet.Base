using EDennis.AspNet.Base.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EDennis.AspNet.Base {

    public static class ModelBuilderExtensions {


        public static ModelBuilder ConfigureCrudEntity<TEntity>(this ModelBuilder modelBuilder)
            where TEntity : CrudEntity {

            var tableName = typeof(TEntity).Name;

            modelBuilder.HasSequence<int>($"seq{tableName}", opt => {
                opt.StartsAt(1)
                .IncrementsBy(1);
            });

            modelBuilder.Entity<TEntity>(e => {
                e.ConfigureTable();
                e.ConfigureSysStatus();
            });

            return modelBuilder;
        }


        public static ModelBuilder ConfigureTemporalEntity<TEntity>(this ModelBuilder modelBuilder)
            where TEntity : TemporalEntity {

            var tableName = typeof(TEntity).Name;

            modelBuilder.HasSequence<int>($"seq{tableName}", opt => {
                opt.StartsAt(1)
                .IncrementsBy(1);
            });

            modelBuilder.Entity<TEntity>(e => {
                e.ConfigureTable();
                e.ConfigureSysStatus();
                e.ConfigureSysStart();
                e.ConfigureSysEnd();
            });

            return modelBuilder;
        }

    }



    public static class EntityTypeBuilderExtensions {

        public static EntityTypeBuilder<TEntity> ConfigureTable<TEntity>(this EntityTypeBuilder<TEntity> e)
            where TEntity : CrudEntity {

            var tableName = typeof(TEntity).Name;

            e.ToTable(tableName)
             .HasKey("Id");

            e.Property("Id")
             .HasDefaultValueSql($"next value for seq{tableName}");

            return e;
        }


        public static EntityTypeBuilder<TEntity> ConfigureSysStatus<TEntity>(this EntityTypeBuilder<TEntity> e)
            where TEntity : CrudEntity {

            e.Property(d => d.SysStatus)
              .HasColumnType("tinyint");

            return e;
        }

        public static EntityTypeBuilder<TEntity> ConfigureSysStart<TEntity>(this EntityTypeBuilder<TEntity> e)
            where TEntity : TemporalEntity {

            e.Property(p => p.SysStart)
             .HasColumnType("datetime2")
             .ValueGeneratedOnAddOrUpdate();

            return e;
        }

        public static EntityTypeBuilder<TEntity> ConfigureSysEnd<TEntity>(this EntityTypeBuilder<TEntity> e)
            where TEntity : TemporalEntity {

            e.Property(p => p.SysEnd)
             .HasColumnType("datetime2")
             .ValueGeneratedOnAddOrUpdate();

            return e;
        }

    }
}
