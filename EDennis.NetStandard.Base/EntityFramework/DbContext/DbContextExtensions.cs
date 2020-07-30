using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.EntityFramework.Entity {
    public static class DbContextExtensions {


        public async static Task<string> GetFromJsonSqlAsync<TContext>(this TContext context, string fromJsonSql)
            where TContext : DbContext {

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

        public async static Task<string> GetFromJsonSqlAsync<TContext>(this TContext context, string fromJsonSql,
            Dictionary<string, object> parameters)
            where TContext : DbContext {

            var sql = $"declare @j varchar(max) = ({fromJsonSql}); select @j json;";
            var cxn = context.Database.GetDbConnection();
            if (cxn.State == ConnectionState.Closed)
                cxn.Open();
            DbCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = sql;
            if (context.Database.CurrentTransaction is IDbContextTransaction trans) {
                cmd.Transaction = trans.GetDbTransaction();
            }
            foreach (var parameter in parameters)
                cmd.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));

            var result = (await cmd.ExecuteScalarAsync()).ToString();
            return result;
        }


    }
}
