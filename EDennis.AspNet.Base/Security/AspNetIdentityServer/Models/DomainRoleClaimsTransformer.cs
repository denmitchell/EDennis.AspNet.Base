using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class DomainRoleClaimsTransformer : IClaimsTransformation {

        private readonly DomainRoleClaimCache _cache;
        private readonly IWebHostEnvironment _env;

        public DomainRoleClaimsTransformer(DomainRoleClaimCache cache, IWebHostEnvironment env) {
            _cache = cache;
            _env = env;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal) {
            var claims =
                await Task.Run(() =>
                {
                    return
                    (from a in principal.Claims.Where(c => c.Type == _env.ApplicationName)
                     join c in _cache
                         on a.Value equals c.RoleName
                     select new Claim(c.ClaimType, c.ClaimValue)
                    ).ToList();
                });

            principal.AddIdentity(new ClaimsIdentity(claims));

            return principal;
        }
    }
}
