using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace EDennis.AspNet.Base {
    public class SqliteDbContextProvider<TContext> : DbContextProvider<TContext>, IDbContextProvider<TContext>
        where TContext : DbContext {


        public SqliteDbContextProvider(TContext context) : base(context) {
        }

        public override DbContextOptions<TContext> GetDbContextOptions(string connectionString, bool autoTransaction, out DbTransaction transaction) {
            var builder = new DbContextOptionsBuilder<TContext>();

            if (autoTransaction) {
                transaction = null;
                builder.UseSqlite(connectionString);
            } else {
                var connection = new SqliteConnection(connectionString);
                connection.Open();
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                builder.UseSqlite(connection);
            }
            return builder.Options;

        }

        public override DbContextOptions<TContext> GetDbContextOptions(DbTransaction transaction) {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlite(transaction.Connection);
            return builder.Options;
        }
    }
}
