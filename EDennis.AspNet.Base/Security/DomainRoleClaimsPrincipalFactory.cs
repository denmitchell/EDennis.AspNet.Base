using IdentityServer4.Test;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class DomainRoleClaimsPrincipalFactory<TUser, TRole> : UserClaimsPrincipalFactory<TUser, TRole>
        where TUser : IdentityUser
        where TRole : DomainRole {

        public DomainRoleClaimsPrincipalFactory(UserManager<TUser> userManager, 
            RoleManager<TRole> roleManager, 
            IOptions<IdentityOptions> options) : base(userManager, roleManager, options) {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user) {
            var identity = await base.GenerateClaimsAsync(user);

            foreach(var role in await UserManager.GetRolesAsync(user)) {
                var components = role.Split('@');
                identity.AddClaim(new Claim(components[0], components[1]));
            }


            return identity;
        }
    }
}
