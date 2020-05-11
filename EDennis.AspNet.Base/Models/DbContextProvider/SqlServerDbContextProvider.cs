using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;

namespace EDennis.AspNet.Base {
    public class SqlServerDbContextProvider<TContext> : DbContextProvider<TContext>, IDbContextProvider<TContext>
        where TContext : DbContext {


        public SqlServerDbContextProvider(TContext context) : base(context) {
        }

        public override DbContextOptions<TContext> GetDbContextOptions(string connectionString, bool autoTransaction, out DbTransaction transaction) {
            var builder = new DbContextOptionsBuilder<TContext>();

            if (autoTransaction) {
                transaction = null;
                builder.UseSqlServer(connectionString);
            } else {
                var connection = new SqlConnection(connectionString);
                connection.Open();
                transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                builder.UseSqlServer(connection);
            }
            return builder.Options;

        }

        public override DbContextOptions<TContext> GetDbContextOptions(DbTransaction transaction) {
            var builder = new DbContextOptionsBuilder<TContext>();
            builder.UseSqlServer(transaction.Connection);
            return builder.Options;
        }
    }
}
