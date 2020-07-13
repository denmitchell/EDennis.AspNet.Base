using EDennis.AspNet.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EDennis.Samples.ColorApi.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class RgbController : TemporalController<ColorContext, Rgb, RgbHistory> {

        public RgbController(ColorContext context, ILogger<QueryController<ColorContext, Rgb>> logger) 
            : base(context, logger) { }

        public override IQueryable<Rgb> Find(string pathParameter) {
            return _dbContext.Rgb.Where(r=>r.Id == int.Parse(pathParameter));
        }


    }
}
