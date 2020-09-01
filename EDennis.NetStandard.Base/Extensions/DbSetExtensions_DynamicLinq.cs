using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;


namespace EDennis.NetStandard.Base {
    public static class DbSetExtensions_DynamicLinq {


        /// <summary>
        /// Gets a dynamic list result using a Dynamic Linq Expression
        /// https://github.com/StefH/System.Linq.Dynamic.Core
        /// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
        /// </summary>
        /// <param name="where">string Where expression</param>
        /// <param name="orderBy">string OrderBy expression (with support for descending)</param>
        /// <param name="skip">int number of records to skip</param>
        /// <param name="take">int number of records to return</param>
        /// <returns>dynamic-typed object</returns>
        public static void GetWithDynamicLinq<TEntity>(
                this DbSet<TEntity> src,
                out DynamicLinqResult<TEntity> result,
                string where = null,
                string orderBy = null,
                string include = null,
                int? skip = null,
                int? take = null,
                int? totalRecords = null
                ) 
                where TEntity : class {

            IQueryable<TEntity> qry = BuildLinqQuery(src, include, where, orderBy, skip, take, totalRecords,
                out DynamicLinqResult<TEntity> dynamicLinqResult);

            var res = qry.ToDynamicList<TEntity>();
            dynamicLinqResult.Data = res;
            result = dynamicLinqResult;
        }



        /// <summary>
        /// Gets a dynamic list result using a Dynamic Linq Expression
        /// https://github.com/StefH/System.Linq.Dynamic.Core
        /// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
        /// </summary>
        /// <param name="where">string Where expression</param>
        /// <param name="orderBy">string OrderBy expression (with support for descending)</param>
        /// <param name="select">string Select expression</param>
        /// <param name="skip">int number of records to skip</param>
        /// <param name="take">int number of records to return</param>
        /// <returns>dynamic-typed object</returns>
        public static void GetWithDynamicLinq<TContext, TEntity>(
                this DbSet<TEntity> src,
                out DynamicLinqResult result,
                string where = null,
                string orderBy = null,
                string select = null,
                string include = null,
                int? skip = null,
                int? take = null,
                int? totalRecords = null
                )
                where TEntity : class {

            IQueryable qry = BuildLinqQuery(src, select, include, where, orderBy, skip, take, totalRecords,
                out DynamicLinqResult dynamicLinqResult);

            var res = qry.ToDynamicList();
            dynamicLinqResult.Data = res;
            result = dynamicLinqResult;

        }




        /// <summary>
        /// Builds a dynamic linq query
        /// </summary>
        /// <param name="where">string Where expression</param>
        /// <param name="orderBy">string OrderBy expression (with support for descending)</param>
        /// <param name="skip">int number of records to skip</param>
        /// <param name="take">int number of records to return</param>
        /// <param name="totalRecords">the total number of records across all pages</param>
        /// <param name="pagedResult">paging metadata</param>
        /// <returns></returns>
        private static IQueryable<TEntity> BuildLinqQuery<TEntity>(DbSet<TEntity> src,
            string include, string where, string orderBy,
            int? skip, int? take, int? totalRecords,
            out DynamicLinqResult<TEntity> pagedResult)
            where TEntity : class {

            var qry = src.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(include)) {
                var includes = include.Split(";");
                foreach (var incl in includes)
                    qry = qry.Include(incl);
            }
            if (!string.IsNullOrWhiteSpace(where)) {
                qry = ApplyWhere(qry, where);
            }
            if (!string.IsNullOrWhiteSpace(orderBy))
                qry = qry.OrderBy(orderBy);


            if (totalRecords == null || totalRecords.Value < 0)
                totalRecords = qry.Count();

            var skipValue = skip == null ? 0 : skip.Value;
            var takeValue = take == null ? totalRecords.Value - skipValue : take.Value;
            var pageCount = (int)Math.Ceiling(totalRecords.Value / (double)takeValue);

            pagedResult = new DynamicLinqResult<TEntity> {
                CurrentPage = 1 + (int)Math.Ceiling((skipValue) / (double)takeValue),
                PageCount = pageCount,
                PageSize = takeValue,
                RowCount = totalRecords.Value
            };
            if (skipValue != 0)
                qry = qry.Skip(skipValue);
            if (take != null && take.Value > 0)
                qry = qry.Take(takeValue);

            return qry;
        }


        /// <summary>
        /// Builds a dynamic linq query
        /// </summary>
        /// <param name="where">string Where expression</param>
        /// <param name="orderBy">string OrderBy expression (with support for descending)</param>
        /// <param name="select">string Select expression</param>
        /// <param name="skip">int number of records to skip</param>
        /// <param name="take">int number of records to return</param>
        /// <param name="totalRecords">the total number of records across all pages</param>
        /// <param name="pagedResult">paging metadata</param>
        /// <returns></returns>
        private static IQueryable BuildLinqQuery<TEntity>(DbSet<TEntity> src, string select, string include, string where,
            string orderBy, int? skip, int? take, int? totalRecords, out DynamicLinqResult pagedResult)
            where TEntity : class {

            IQueryable<TEntity> qry = BuildLinqQuery(src, include, where, orderBy, skip, take, totalRecords, out DynamicLinqResult<TEntity> pagedResultInner);

            pagedResult = new DynamicLinqResult {
                CurrentPage = pagedResultInner.CurrentPage,
                PageCount = pagedResultInner.PageCount,
                PageSize = pagedResultInner.PageSize,
                RowCount = pagedResultInner.RowCount
            };

            if (!string.IsNullOrWhiteSpace(select))
                return qry.AsQueryable().Select(select);
            else
                return qry.AsQueryable();
        }


        private static IQueryable<TEntity> ApplyWhere<TEntity>(IQueryable<TEntity> qry, string where)
            where TEntity : class {

            (qry, where) = DynamicLinqCaseInsensitiveConditionBuilder<TEntity>.ParseApplyStringConditions(qry, where);

            if (!string.IsNullOrEmpty(where))
                qry = qry.Where(where);

            return qry;
        }


    }
}
