using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.EntityFramework.Entity {
    public static class DbContextExtensions {
        public async static Task<string> GetFromJsonSqlAsync<TContext>(this TContext context, string fromJsonSql)
            where TContext: DbContext {

            var sql = $"declare @j varchar(max) = ({fromJsonSql}); select @j json;";
            var cxn = context.Database.GetDbConnection();
            if (cxn.State == ConnectionState.Closed)
                cxn.Open();
            string result;
            if (context.Database.CurrentTransaction is IDbContextTransaction trans) {
                var dbTrans = trans.GetDbTransaction();
                result = await cxn.ExecuteScalarAsync<string>(sql, transaction: dbTrans);
            } else {
                result = await cxn.ExecuteScalarAsync<string>(sql);
            }
            return result;
        }
    }
}
