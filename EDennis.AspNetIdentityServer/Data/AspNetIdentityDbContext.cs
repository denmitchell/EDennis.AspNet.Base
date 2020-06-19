using EDennis.AspNetIdentityServer.Models;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EDennis.AspNetIdentityServer.Data {
    public class AspNetIdentityDbContext : IdentityDbContext<AspNetIdentityUser> {
        public AspNetIdentityDbContext(DbContextOptions<AspNetIdentityDbContext> options)
            : base(options) { }
    }
}
