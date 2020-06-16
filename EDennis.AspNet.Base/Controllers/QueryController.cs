using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Exceptions;
using System.Text.Json;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EDennis.AspNet.Base {
    [Route("api/[controller]")]
    [ApiController]
    public class QueryController<TContext, TEntity> : ControllerBase 
        where TContext : DbContext
        where TEntity : class {

        protected readonly TContext _dbContext;

        public QueryController(TContext context) {
            _dbContext = context;
        }


        public virtual void AdjustQuery(ref IQueryable<TEntity> query) { }



        /// <summary>
        /// Get from DevExtreme DataSourceLoader query string
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        [HttpGet("devextreme")]
        public virtual IActionResult GetWithDevExtreme(
                [FromQuery] string select,
                [FromQuery] string sort,
                [FromQuery] string filter,
                [FromQuery] int skip,
                [FromQuery] int take,
                [FromQuery] string totalSummary,
                [FromQuery] string group,
                [FromQuery] string groupSummary
            ) {
            DataSourceLoadOptions loadOptions;
            try {
                loadOptions = DataSourceLoadOptionsBuilder.Build(
                    select, sort, filter, skip, take, totalSummary,
                    group, groupSummary);
            } catch (ArgumentException ex) {
                ModelState.AddModelError("", ex.Message);
                return new BadRequestObjectResult(ModelState);
            }
            try {
                var result = DataSourceLoader.Load(GetQuery(), loadOptions);
                return Ok(result);
            } catch (ArgumentOutOfRangeException) {
                var obj =
                    new {
                        Exception = "Failed executing DevExtreme load expression",
                        ProvidedArguments = new {
                            select, sort, filter, skip, take, totalSummary, group, groupSummary
                        }
                    };
                return new BadRequestObjectResult(obj);
            }
        }


        /// <summary>
        /// Asynchronously get from DevExtreme DataSourceLoader query string
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        [HttpGet("devextreme/async")]
        public virtual async Task<IActionResult> GetWithDevExtremeAsync(
                [FromQuery] string select,
                [FromQuery] string sort,
                [FromQuery] string filter,
                [FromQuery] int skip,
                [FromQuery] int take,
                [FromQuery] string totalSummary,
                [FromQuery] string group,
                [FromQuery] string groupSummary
            ) {
            var loadOptions = DataSourceLoadOptionsBuilder.Build(
                select, sort, filter, skip, take, totalSummary,
                group, groupSummary);

            return await Task.Run(() => GetWithDevExtreme(select, sort, filter, skip, take, totalSummary, group, groupSummary));
        }





        /// <summary>
        /// Get by Dynamic Linq Expression
        /// https://github.com/StefH/System.Linq.Dynamic.Core
        /// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
        /// </summary>
        /// <param name="where">string Where expression</param>
        /// <param name="orderBy">string OrderBy expression (with support for descending)</param>
        /// <param name="select">string Select expression</param>
        /// <param name="skip">int number of records to skip</param>
        /// <param name="take">int number of records to return</param>
        /// <returns>dynamic-typed object</returns>
        [HttpGet("linq")]
        public virtual IActionResult GetWithDynamicLinq(
                [FromQuery] string where = null,
                [FromQuery] string orderBy = null,
                [FromQuery] string select = null,
                [FromQuery] int? skip = null,
                [FromQuery] int? take = null,
                [FromQuery] int? totalRecords = null
                ) {

            try {
                if (select != null) {

                    IQueryable qry = BuildLinqQuery(select, where, orderBy, skip, take, totalRecords,
                        out DynamicLinqResult dynamicLinqResult);

                    var result = qry.ToDynamicList();
                    dynamicLinqResult.Data = result;
                    var json = JsonSerializer.Serialize(dynamicLinqResult);
                    return new ContentResult { Content = json, ContentType = "application/json" };
                } else {
                    IQueryable<TEntity> qry = BuildLinqQuery(where, orderBy, skip, take, totalRecords,
                        out DynamicLinqResult<TEntity> dynamicLinqResult);

                    var result = qry.ToDynamicList<TEntity>();
                    dynamicLinqResult.Data = result;

                    var json = JsonSerializer.Serialize(dynamicLinqResult);
                    return new ContentResult { Content = json, ContentType = "application/json" };
                }
            } catch (ParseException ex) {
                ModelState.AddModelError("", ex.Message);
                return new BadRequestObjectResult(ModelState);
            }

        }



        /// <summary>
        /// Get by Dynamic Linq Expression
        /// https://github.com/StefH/System.Linq.Dynamic.Core
        /// https://github.com/StefH/System.Linq.Dynamic.Core/wiki/Dynamic-Expressions
        /// </summary>
        /// <param name="where">string Where expression</param>
        /// <param name="orderBy">string OrderBy expression (with support for descending)</param>
        /// <param name="select">string Select expression</param>
        /// <param name="skip">int number of records to skip</param>
        /// <param name="take">int number of records to return</param>
        /// <returns>dynamic-typed object</returns>
        [HttpGet("linq/async")]
        public virtual async Task<IActionResult> GetWithDynamicLinqAsync(
                [FromQuery] string where = null,
                [FromQuery] string orderBy = null,
                [FromQuery] string select = null,
                [FromQuery] int? skip = null,
                [FromQuery] int? take = null,
                [FromQuery] int? totalRecords = null
                ) {
            try {
                if (select != null) {

                    IQueryable qry = BuildLinqQuery(select, where, orderBy, skip, take, totalRecords,
                        out DynamicLinqResult dynamicLinqResult);

                    var result = await qry.ToDynamicListAsync();
                    dynamicLinqResult.Data = result;
                    var json = JsonSerializer.Serialize(dynamicLinqResult);
                    return new ContentResult { Content = json, ContentType = "application/json" };
                } else {
                    IQueryable<TEntity> qry = BuildLinqQuery(where, orderBy, skip, take, totalRecords,
                        out DynamicLinqResult<TEntity> dynamicLinqResult);

                    var result = await qry.ToDynamicListAsync<TEntity>();
                    dynamicLinqResult.Data = result;

                    var json = JsonSerializer.Serialize(dynamicLinqResult);
                    return new ContentResult { Content = json, ContentType = "application/json" };
                }
            } catch (ParseException ex) {
                ModelState.AddModelError("", ex.Message);
                return new BadRequestObjectResult(ModelState);
            }
        }




        private IQueryable<TEntity> BuildLinqQuery(string where, string orderBy, int? skip, int? take, int? totalRecords, out DynamicLinqResult<TEntity> pagedResult) {

            var qry  = GetQuery();

            try {
                if (!string.IsNullOrWhiteSpace(where))
                    qry = qry.Where(where);
                if (!string.IsNullOrWhiteSpace(orderBy))
                    qry = qry.OrderBy(orderBy);
            } catch (ParseException ex) {
                throw new ArgumentException(ex.Message);
            }

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



        private IQueryable BuildLinqQuery(string select, string where, string orderBy, int? skip, int? take, int? totalRecords, out DynamicLinqResult pagedResult) {

            IQueryable<TEntity> qry = BuildLinqQuery(where, orderBy, skip, take, totalRecords, out DynamicLinqResult<TEntity> pagedResultInner);

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



        private IQueryable<TEntity> GetQuery(){
            var qry = _dbContext
                .Set<TEntity>()
                .AsNoTracking();

            AdjustQuery(ref qry);
            return qry;
        }


    }
}
