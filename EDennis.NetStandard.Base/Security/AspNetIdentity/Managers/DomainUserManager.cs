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
        public DomainUserManager(DomainUserStore store, 
            IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<DomainUser> passwordHasher, 
            IEnumerable<IUserValidator<DomainUser>> userValidators, 
            IEnumerable<IPasswordValidator<DomainUser>> passwordValidators, 
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, 
            IServiceProvider services, ILogger<UserManager<DomainUser>> logger, 
            DomainIdentityDbContext<DomainUser, DomainRole> dbContext,
            IAppClaimEncoder encoder) 
            : base(store, optionsAccessor, passwordHasher, userValidators, 
                  passwordValidators, keyNormalizer, errors, services, logger, dbContext, encoder) {
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


        /// <summary>
        /// Adds a user to a role.  This method is overriden from the base class
        /// to take into consideration application name.  To work with this method,
        /// application name must be incorporated into role name to produce a roleString.
        /// The nature of the incorporation is defined by the IAppClaimEncoder implementation
        /// </summary>
        /// <param name="user">the user to whom to add the role</param>
        /// <param name="roleString">Combined application name and role name</param>
        /// <returns>ASP.NET IdentityResult</returns>
        public override async Task<IdentityResult> AddToRoleAsync(TUser user, string roleString) {
            var appRole = _encoder.Decode(roleString);
            var role = await _dbContext.Set<TRole>()
                .FirstOrDefaultAsync(r => r.Application == appRole.Application 
                && r.NormalizedName == appRole.Application.ToUpper());

            if (role == null)
                return IdentityResult.Failed(new IdentityErrorDescriber()
                    .RoleNotFoundError(appRole.Application, appRole.RoleNomen));
            else {
                var exists = _dbContext.Set<IdentityUserRole<int>>().Any(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
                if (exists)
                    return IdentityResult.Failed(new IdentityErrorDescriber()
                        .UserAlreadyInRole(roleString));
                else {
                    _dbContext.Set<IdentityUserRole<int>>().Add(new IdentityUserRole<int> { UserId = user.Id, RoleId = role.Id });
                    await _dbContext.SaveChangesAsync();
                    return IdentityResult.Success;
                }
            }
        }


        /// <summary>
        /// Adds a user to a set of roles.  This method is overriden from the base class
        /// to take into consideration application name.  To work with this method,
        /// application name must be incorporated into role name to produce a roleString.
        /// The nature of the incorporation is defined by the IAppClaimEncoder implementation
        /// </summary>
        /// <param name="user">the user to whom to add the role</param>
        /// <param name="roleString">Combined application name and role name</param>
        /// <returns>ASP.NET IdentityResult</returns>
        public override async Task<IdentityResult> AddToRolesAsync(TUser user, IEnumerable<string> roleStrings) {
            var errors = new List<IdentityError>();

            foreach(var roleString in roleStrings) {
                var result = await AddToRoleAsync(user, roleString);
                if (!result.Succeeded)
                    errors.AddRange(result.Errors);
            }
            if (errors.Count > 0)
                return IdentityResult.Failed(errors.ToArray());
            else
                return IdentityResult.Success;

        }



        /// <summary>
        /// Removes a user from a role.  This method is overriden from the base class
        /// to take into consideration application name.  To work with this method,
        /// application name must be incorporated into role name to produce a roleString.
        /// The nature of the incorporation is defined by the IAppClaimEncoder implementation
        /// </summary>
        /// <param name="user">the user to whom to add the role</param>
        /// <param name="roleString">Combined application name and role name</param>
        /// <returns>ASP.NET IdentityResult</returns>
        public override async Task<IdentityResult> RemoveFromRoleAsync(TUser user, string roleString) {
            var appRole = _encoder.Decode(roleString);
            var role = await _dbContext.Set<TRole>()
                .FirstOrDefaultAsync(r => r.Application == appRole.Application
                && r.NormalizedName == appRole.Application.ToUpper());

            if (role == null)
                return IdentityResult.Failed(new IdentityErrorDescriber()
                    .RoleNotFoundError(appRole.Application, appRole.RoleNomen));
            else {
                var exists = _dbContext.Set<IdentityUserRole<int>>().Any(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
                if (!exists)
                    return IdentityResult.Failed(new IdentityErrorDescriber()
                        .UserNotInRole(roleString));
                else {
                    _dbContext.Set<IdentityUserRole<int>>().Remove(new IdentityUserRole<int> { UserId = user.Id, RoleId = role.Id });
                    await _dbContext.SaveChangesAsync();
                    return IdentityResult.Success;
                }
            }
        }


        /// <summary>
        /// Removes a user from a set of roles.  This method is overriden from the base class
        /// to take into consideration application name.  To work with this method,
        /// application name must be incorporated into role name to produce a roleString.
        /// The nature of the incorporation is defined by the IAppClaimEncoder implementation
        /// </summary>
        /// <param name="user">the user to whom to add the role</param>
        /// <param name="roleString">Combined application name and role name</param>
        /// <returns>ASP.NET IdentityResult</returns>
        public override async Task<IdentityResult> RemoveFromRolesAsync(TUser user, IEnumerable<string> roleStrings) {
            var errors = new List<IdentityError>();

            foreach (var roleString in roleStrings) {
                var result = await RemoveFromRoleAsync(user, roleString);
                if (!result.Succeeded)
                    errors.AddRange(result.Errors);
            }
            if (errors.Count > 0)
                return IdentityResult.Failed(errors.ToArray());
            else
                return IdentityResult.Success;

        }



        /// <summary>
        /// Returns a list of roles (as roleStrings) for a given user.
        /// Each role string is combination of application name and role name
        /// as implemented by the IAppClaimEncoder.Encode(AppRole) method
        /// </summary>
        /// <param name="user">The user for whom roles are returned</param>
        /// <returns>a list of role strings that represent role name and application name</returns>
        public override async Task<IList<string>> GetRolesAsync(TUser user) {

            var roles = _dbContext.Set<TRole>().FromSqlInterpolated($@"
SELECT r.* 
    FROM AspNetRoles r
    WHERE EXISTS (
        SELECT 0  
            FROM AspNetUserRoles ur
            WHERE ur.UserId = {user.Id}
                and ur.RoleId = r.Id
)");
            return (await roles.ToListAsync())
                .Select(r => _encoder.Encode(new AppRole { Application = r.Application, RoleNomen = r.Name }))
                .ToList();
        }




    }
}
