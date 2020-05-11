using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;

namespace EDennis.AspNet.Base {
    public abstract class DbContextProvider<TContext> : IDbContextProvider<TContext> 
        where TContext : DbContext {

        public TContext DbContext { get; private set; }

        public DbContextProvider(TContext context) {
            DbContext = context;
        }

        public abstract DbContextOptions<TContext> GetDbContextOptions(string connectionString, bool autoTransaction, out DbTransaction transaction);
        public abstract DbContextOptions<TContext> GetDbContextOptions(DbTransaction transaction);



        public void SetDbContext(DbTransaction transaction) {
            var options = GetDbContextOptions(transaction);
            var context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options });

            context.Database.AutoTransactionsEnabled = false;
            context.Database.UseTransaction(transaction);

            DbContext = context;
        }


        public void SetDbContext(string connectionString, bool autoTransaction) {

            var options = GetDbContextOptions(connectionString, autoTransaction, out DbTransaction transaction);
            var context = (TContext)Activator.CreateInstance(typeof(TContext), new object[] { options });

            if (!autoTransaction) {
                context.Database.AutoTransactionsEnabled = false;
                context.Database.UseTransaction(transaction);
            }
            DbContext = context;
        }
    }
}
