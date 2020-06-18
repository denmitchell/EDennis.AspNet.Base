using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Middleware {
    public class JwtSigningCredentials {
        public SigningCredentials SigningCredentials { get; set; }
    }
}
