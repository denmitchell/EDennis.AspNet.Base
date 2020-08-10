using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq.Expressions;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Extensions to simplify configuration of entities in the DbContext class
    /// </summary>
    public static class ModelBuilderExtensions {


        public const int SYSUSER_MAX_LENGTH = 150;

        /// <summary>
        /// Configures a CrudEntity using an Entity Framework ModelBuilder
        /// </summary>
        /// <typeparam name="TEntity">the entity's class</typeparam>
        /// <param name="modelBuilder">Entity Framework ModelBuilder</param>
        /// <param name="keyExpression">Primary key lambda</param>
        /// <param name="useSequence">Whether to configure a (SQL Server) sequence</param>
        /// <returns></returns>
        public static ModelBuilder ConfigureCrudEntity<TEntity>(this ModelBuilder modelBuilder,
            Expression<Func<TEntity, object>> keyExpression, bool useSequence = true, string tableName = null)
            where TEntity : class, ICrudEntity {

            modelBuilder.Entity<TEntity>(e => {
                e.ConfigureTable(modelBuilder, keyExpression, useSequence, tableName);
                e.ConfigureSysUser();
                e.ConfigureSysStatus();
            });

            return modelBuilder;
        }



        /// <summary>
        /// Configures a TemporalEntity using an Entity Framework ModelBuilder
        /// </summary>
        /// <typeparam name="TEntity">the entity's class</typeparam>
        /// <param name="modelBuilder">Entity Framework ModelBuilder</param>
        /// <param name="keyExpression">Primary key lambda</param>
        /// <param name="useSequence">Whether to configure a (SQL Server) sequence</param>
        /// <param name="isSqlServerTemporal">Whether to provision System-Versioned tables in SQL Server.
        /// NOTE: this requires replacing the SqlServerMigrationsSqlGenerator service
        /// with MigrationsExtensionsSqlGenerator from EDennis.MigrationsExtensions</param>
        /// <returns></returns>
        public static ModelBuilder ConfigureTemporalEntity<TEntity>(this ModelBuilder modelBuilder,                 
                Expression<Func<TEntity,object>> keyExpression, bool useSequence = true,
                bool isSqlServerTemporal = true, string tableName = null)
            where TEntity : class, ITemporalEntity {


            modelBuilder.Entity<TEntity>(e => {
                if (isSqlServerTemporal)
                    e.HasAnnotation("SystemVersioned", true);
                e.ConfigureTable(modelBuilder, keyExpression, useSequence, tableName);
                e.ConfigureSysUser();
                e.ConfigureSysStatus();
                e.ConfigureSysStart();
                e.ConfigureSysEnd();
            });

            return modelBuilder;
        }



    }



    public static class EntityTypeBuilderExtensions {



        /// <summary>
        /// Configures a CrudEntity using an Entity Framework ModelBuilder
        /// </summary>
        /// <typeparam name="TEntity">the entity's class</typeparam>
        /// <param name="builder">Entity Framework EntityTypeBuilder</param>
        /// <param name="modelBuilder">Entity Framework ModelBuilder</param>
        /// <param name="keyExpression">Primary key lambda</param>
        /// <param name="useSequence">Whether to configure a (SQL Server) sequence</param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> ConfigureCrudEntity<TEntity>(this EntityTypeBuilder<TEntity> builder,
            ModelBuilder modelBuilder,
            Expression<Func<TEntity, object>> keyExpression, bool useSequence = true, string tableName = null)
            where TEntity : class, ICrudEntity {

            builder.ConfigureTable(modelBuilder, keyExpression, useSequence, tableName)
                .ConfigureSysUser()
                .ConfigureSysStatus();

            return builder;
        }



        /// <summary>
        /// Configures a TemporalEntity using an Entity Framework ModelBuilder
        /// </summary>
        /// <typeparam name="TEntity">the entity's class</typeparam>
        /// <param name="builder">Entity Framework EntityTypeBuilder</param>
        /// <param name="modelBuilder">Entity Framework ModelBuilder</param>
        /// <param name="keyExpression">Primary key lambda</param>
        /// <param name="useSequence">Whether to configure a (SQL Server) sequence</param>
        /// <param name="isSqlServerTemporal">Whether to provision System-Versioned tables in SQL Server.
        /// NOTE: this requires replacing the SqlServerMigrationsSqlGenerator service
        /// with MigrationsExtensionsSqlGenerator from EDennis.MigrationsExtensions</param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> ConfigureTemporalEntity<TEntity>(this EntityTypeBuilder<TEntity> builder,
                ModelBuilder modelBuilder,
                Expression<Func<TEntity, object>> keyExpression, bool useSequence = true,
                bool isSqlServerTemporal = true, string tableName = null)
            where TEntity : class, ITemporalEntity {


            if (isSqlServerTemporal)
                builder.HasAnnotation("SystemVersioned", true);
            builder.ConfigureTable(modelBuilder, keyExpression, useSequence, tableName);
            builder.ConfigureSysUser();
            builder.ConfigureSysStatus();
            builder.ConfigureSysStart();
            builder.ConfigureSysEnd();

            return builder;
        }



        /// <summary>
        /// Configures a table from an Entity
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="e">The entity</param>
        /// <param name="modelBuilder">Entity Framework ModelBuilder</param>
        /// <param name="keyExpression">Primary key lambda</param>
        /// <param name="useSequence">Whether to configure a (SQL Server) sequence</param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> ConfigureTable<TEntity>(this EntityTypeBuilder<TEntity> e, 
            ModelBuilder modelBuilder, 
            Expression<Func<TEntity, object>> keyExpression, bool useSequence = true, string tableName = null )
            
            where TEntity : class, ICrudEntity {

            tableName ??= typeof(TEntity).Name;

            e.ToTable(tableName)
             .HasKey(keyExpression);

            if (useSequence) {
                modelBuilder.HasSequence<int>($"seq{tableName}", opt => {
                    opt.StartsAt(1)
                    .IncrementsBy(1);
                });
                e.Property(keyExpression)
                 .HasDefaultValueSql($"next value for seq{tableName}");
            }

            return e;
        }


        /// <summary>
        /// Configures SysStatus using a TINYINT database type
        /// </summary>
        /// <typeparam name="TEntity">the entity type</typeparam>
        /// <param name="e">the entity type builder</param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> ConfigureSysStatus<TEntity>(this EntityTypeBuilder<TEntity> e)
            where TEntity : class, ICrudEntity {

            e.Property(d => d.SysStatus)
              .HasColumnType("tinyint")
              .HasDefaultValue(SysStatus.Normal);

            return e;
        }


        /// <summary>
        /// Configures SysStatus using a TINYINT database type
        /// </summary>
        /// <typeparam name="TEntity">the entity type</typeparam>
        /// <param name="e">the entity type builder</param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> ConfigureSysUser<TEntity>(this EntityTypeBuilder<TEntity> e)
            where TEntity : class, ICrudEntity {

            e.Property(d => d.SysUser)
              .HasMaxLength(ModelBuilderExtensions.SYSUSER_MAX_LENGTH);

            return e;
        }



        /// <summary>
        /// Configures SysStart using a DATETIME2 database type
        /// </summary>
        /// <typeparam name="TEntity">the entity type</typeparam>
        /// <param name="e">the entity type builder</param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> ConfigureSysStart<TEntity>(this EntityTypeBuilder<TEntity> e)
            where TEntity : class, ITemporalEntity {

            e.Property(p => p.SysStart)
             .HasColumnType("datetime2")
             .ValueGeneratedOnAddOrUpdate();

            return e;
        }

        /// <summary>
        /// Configures SysEnd using a DATETIME2 database type
        /// </summary>
        /// <typeparam name="TEntity">the entity type</typeparam>
        /// <param name="e">the entity type builder</param>
        /// <returns></returns>
        public static EntityTypeBuilder<TEntity> ConfigureSysEnd<TEntity>(this EntityTypeBuilder<TEntity> e)
            where TEntity : class, ITemporalEntity {

            e.Property(p => p.SysEnd)
             .HasColumnType("datetime2")
             .ValueGeneratedOnAddOrUpdate();

            return e;
        }

    }
}
