using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin.UserApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Admin.UserApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase {

        public void AddUser(User user) { }
        
    }
}
