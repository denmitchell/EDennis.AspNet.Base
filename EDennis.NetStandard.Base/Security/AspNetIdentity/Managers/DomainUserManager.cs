using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {


    public class DomainUserManager : DomainUserManager<DomainUser, DomainRole> {
        public DomainUserManager(IUserStore<DomainUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<DomainUser> passwordHasher, IEnumerable<IUserValidator<DomainUser>> userValidators, IEnumerable<IPasswordValidator<DomainUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<DomainUser>> logger, DomainIdentityDbContext<DomainUser, DomainRole> dbContext) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger, dbContext) {
        }
    }


    public class DomainUserManager<TUser, TRole> : UserManager<TUser>
        where TUser : DomainUser
        where TRole : DomainRole {

        private readonly DomainIdentityDbContext<TUser, TRole> _dbContext;
        private readonly IAppClaimEncoder _encoder;

        public DomainUserManager(IUserStore<TUser> store, 
            IOptions<IdentityOptions> optionsAccessor, 
            IPasswordHasher<TUser> passwordHasher, 
            IEnumerable<IUserValidator<TUser>> userValidators, 
            IEnumerable<IPasswordValidator<TUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, 
            IdentityErrorDescriber errors, 
            IServiceProvider services, 
            ILogger<UserManager<TUser>> logger,
            DomainIdentityDbContext<TUser,TRole> dbContext,
            IAppClaimEncoder encoder) 
            : base(store, optionsAccessor, passwordHasher, userValidators, 
                  passwordValidators, keyNormalizer, errors, services, logger) {

            _dbContext = dbContext;
            _encoder = encoder;
        }

        public virtual async Task<ICollection<TUser>> GetUsersForOrganizationAsync(string organization, int pageNumber = 1, int pageSize = 100) {
            return await Users.Where(u => u.Organization == organization)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public virtual async Task<ICollection<TUser>> GetUsersForApplicationAsync(string application, int pageNumber = 1, int pageSize = 100) {
            var qry = _dbContext.Set<TUser>().FromSqlInterpolated(
                $@"
SELECT u.* FROM AspNetUsers u
    WHERE EXISTS (    
        SELECT 0 
            FROM AspNetRoles r
            INNER JOIN AspNetUserRoles ur
                on r.Id = ur.RoleId
            WHERE r.Application = {application}
                AND u.Id = ur.UserId   
    )
    ORDER BY u.UserName
    OFFSET {(pageNumber - 1) * pageSize} FETCH NEXT {pageSize} ROWS ONLY
");
            return await qry.ToListAsync();              
        }


        public virtual async Task<DomainUserView> GetUserViewAsync(string userName) {
            return await _dbContext.Set<DomainUserView>()
                .FirstOrDefaultAsync(u => u.NormalizedUserName == userName.ToUpper());
        }

        public virtual async Task<DomainUserView> GetUserViewAsync(int id) {
            return await _dbContext.Set<DomainUserView>()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public virtual async Task<ICollection<DomainUserView>> GetUserViewForOrganizationAsync(string organization, int pageNumber = 1, int pageSize = 100) {
            return await _dbContext.Set<DomainUserView>()
                .Where(u => u.Organization == organization)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public virtual async Task<ICollection<DomainUserView>> GetUserViewForApplicationAsync(string application, int pageNumber = 1, int pageSize = 100) {
            var qry = _dbContext.Set<DomainUserView>().FromSqlInterpolated(
                $@"
SELECT u.* FROM AspNetUsers u
    WHERE EXISTS (    
        SELECT 0 
            FROM AspNetRoles r
            INNER JOIN AspNetUserRoles ur
                on r.Id = ur.RoleId
            WHERE r.Application = {application}
                AND u.Id = ur.UserId   
    )
    ORDER BY u.UserName
    OFFSET {(pageNumber - 1) * pageSize} FETCH NEXT {pageSize} ROWS ONLY
");
            return await qry.ToListAsync();
        }


        public virtual async Task<TUser> FindByIdAsync(int id) {
            return await Users.FirstOrDefaultAsync(u => u.Id == id);
        }



        public override Task<IdentityResult> AddToRoleAsync(TUser user, string roleString) {
            var AppRole = _encoder.Decode(roleString);
            return base.AddToRoleAsync(user, role);
        }







    }
}
