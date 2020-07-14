using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base {

    [Route("api/[controller]")]
    [ApiController]
    public abstract class TemporalController<TContext, TEntity, THistoryEntity> : CrudController<TContext, TEntity>
        where TContext : DbContext
        where TEntity : TemporalEntity
        where THistoryEntity : TemporalEntity {

        public TemporalController(TContext context, ILogger<QueryController<TContext,TEntity>> logger) 
            : base(context, logger) { }

        [NonAction]
        protected override void BeforeUpdate(TEntity input) {
            var now = DateTime.Now;

            //write old record to history
            WriteToHistory(input, input.SysStatus, input.SysUser, now);
            input.SysStart = now;
            input.SysEnd = DateTime.MaxValue;
        }


        [NonAction]
        protected override void BeforeDelete(TEntity existing) {
            var now = DateTime.Now;

            //write existing record to history
            WriteToHistory(existing, existing.SysStatus, existing.SysUser, now);
            existing.SysStart = now;
            existing.SysEnd = DateTime.MaxValue;

            now = DateTime.Now;

            //write history record with Deleted Status and Deleting User
            WriteToHistory(existing, SysStatus.Deleted, _sysUser, now);
            existing.SysStart = now;
            existing.SysEnd = DateTime.MaxValue;

        }

        private void WriteToHistory(TEntity entity, SysStatus status, string user, DateTime now) {
            if (entity.SysStatus == SysStatus.Normal) {
                var historyEntity = TemporalEntity.CloneFrom<TEntity, THistoryEntity>(entity);
                historyEntity.SysStatus = status;
                historyEntity.SysUser = user;
                historyEntity.SysEnd = now.AddTicks(-1);
                Task.Run(() => {
                    _dbContext.Add(historyEntity);
                    _dbContext.SaveChanges();
                });
            }
        }

    }
}
