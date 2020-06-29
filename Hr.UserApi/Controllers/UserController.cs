using Admin.UserApi.Models;
using EDennis.AspNet.Base;
using EDennis.AspNetIdentityServer.Data;
using EDennis.AspNetIdentityServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Admin.UserApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : CrudController<AspNetIdentityDbContext, AspNetIdentityUser> {
        
        public UserController(AspNetIdentityDbContext context, 
            ILogger<QueryController<AspNetIdentityDbContext, AspNetIdentityUser>> logger) 
            : base(context, logger) {
        }

        public override IQueryable<AspNetIdentityUser> Find(string pathParameter) {
            if (pathParameter.Contains("@"))
                return _dbContext.Set<AspNetIdentityUser>().Where(u=>u.Email == pathParameter);
            else
                return _dbContext.Set<AspNetIdentityUser>().Where(u => u.Id == pathParameter);
        }

    }
}
