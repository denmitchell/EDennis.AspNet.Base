using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Security.Claims;

namespace EDennis.AspNet.Base {
    public interface IDbContextProvider<TContext> where TContext : DbContext {
        TContext DbContext { get; }

        DbContextOptions<TContext> GetDbContextOptions(DbTransaction transaction);
        DbContextOptions<TContext> GetDbContextOptions(string connectionString, bool autotransaction, out DbTransaction transaction);
        void SetDbContext(DbTransaction transaction);
        void SetDbContext(string connectionString, bool autoTransaction);
    }
}