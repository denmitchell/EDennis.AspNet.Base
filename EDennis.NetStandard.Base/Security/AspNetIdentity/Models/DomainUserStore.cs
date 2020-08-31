using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class DomainUserStore : UserStoreBase<DomainUser, int, IdentityUserClaim<int>, IdentityUserLogin<int>, IdentityUserToken<int>> {

        public DomainIdentityDbContext _dbContext;
        private readonly ILogger<DomainUserStore> _logger;

        public DomainUserStore(DomainIdentityDbContext dbContext, 
            IdentityErrorDescriber describer,
            ILogger<DomainUserStore> logger
            ) : base(describer) {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override IQueryable<DomainUser> Users => _dbContext.Users.AsNoTracking();


        public override async Task SetLockoutEndDateAsync(DomainUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default) {    
            await base.SetLockoutEndDateAsync(user, lockoutEnd, cancellationToken);

            //if this method is called with a lockoutEnd date in the future, 
            //the user should be locked out; so, set the LockoutBegin = now
            if (lockoutEnd > DateTime.Now)
                user.LockoutBegin = DateTime.Now;
        }



        public override async Task AddClaimsAsync(DomainUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainUserStore.AddClaimsAsync failed with {Code}: {Description}";
            var userClaims = claims.Select(c => new IdentityUserClaim<int> { UserId = user.Id, ClaimType = c.Type, ClaimValue = c.Value });
            try {
                var existingClaims = await _dbContext.Set<IdentityUserClaim<int>>()
                    .Where(uc => uc.UserId == user.Id)
                    .Select(uc => new IdentityUserClaim<int> { UserId = uc.UserId, ClaimType = uc.ClaimType, ClaimValue = uc.ClaimValue })
                    .ToListAsync(cancellationToken);
                var claimsToAdd = userClaims.Except(existingClaims);
                _dbContext.Set<IdentityUserClaim<int>>().AddRange(claimsToAdd);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }

        public override async Task<IdentityResult> CreateAsync(DomainUser user, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainUserStore.CreateAsync failed with {Code}: {Description}";
            try {
                _dbContext.Set<DomainUser>().Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                return IdentityResult.Failed(err);
            }
            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> UpdateAsync(DomainUser user, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainUserStore.UpdateAsync failed with {Code}: {Description}";
            try {
                _dbContext.Set<DomainUser>().Update(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                return IdentityResult.Failed(err);
            }
            return IdentityResult.Success;
        }


        public override async Task<IdentityResult> DeleteAsync(DomainUser user, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainUserStore.DeleteAsync failed with {Code}: {Description}";
            try {
                _dbContext.Set<DomainUser>().Remove(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                return IdentityResult.Failed(err);
            }
            return IdentityResult.Success;
        }

        public override async Task<DomainUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default) {
            if (!int.TryParse(userId, out int userIdInt))
                return null;
            return await _dbContext.Set<DomainUser>().FirstOrDefaultAsync(e => e.Id == userIdInt, cancellationToken);
        }

        public override async Task<DomainUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default) {
            return await _dbContext.Set<DomainUser>()
                .FirstOrDefaultAsync(e => EF.Functions.Like(e.NormalizedUserName, normalizedUserName), cancellationToken);
        }

        protected override async Task<DomainUser> FindUserAsync(int userId, CancellationToken cancellationToken) {
            return await _dbContext.Set<DomainUser>().FindAsync(userId, cancellationToken);
        }

        protected override async Task<IdentityUserLogin<int>> FindUserLoginAsync(int userId, string loginProvider, string providerKey, CancellationToken cancellationToken) {
            return await _dbContext.Set<IdentityUserLogin<int>>().FirstOrDefaultAsync(
                ul => ul.UserId == userId && ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey,
                cancellationToken);
        }

        protected override async Task<IdentityUserLogin<int>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) {
            return await _dbContext.Set<IdentityUserLogin<int>>().FirstOrDefaultAsync(
                            ul => ul.LoginProvider == loginProvider && ul.ProviderKey == providerKey,
                            cancellationToken);
        }

        public override async Task<IList<Claim>> GetClaimsAsync(DomainUser user, CancellationToken cancellationToken = default) {
            return (await _dbContext.Set<IdentityUserClaim<int>>()
                .Where(uc => uc.UserId == user.Id)
                .ToListAsync(cancellationToken))
                .Select(uc => new Claim(uc.ClaimType, uc.ClaimValue))
                .ToList();
        }

        public override async Task ReplaceClaimAsync(DomainUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainUserStore.ReplaceClaimAsync failed with {Code}: {Description}";
            var userClaim = new IdentityUserClaim<int> { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value };
            try {
                var existingClaim = await _dbContext.Set<IdentityUserClaim<int>>()
                    .FirstOrDefaultAsync(uc => uc.UserId == user.Id, cancellationToken);
                _dbContext.Set<IdentityUserClaim<int>>().Remove(existingClaim);
                _dbContext.Set<IdentityUserClaim<int>>().Add(userClaim);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }

        public override async Task RemoveClaimsAsync(DomainUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainUserStore.RemoveClaimsAsync failed with {Code}: {Description}";
            var userClaims = claims.Select(c => new IdentityUserClaim<int> { UserId = user.Id, ClaimType = c.Type, ClaimValue = c.Value });
            try {
                var existingClaims = await _dbContext.Set<IdentityUserClaim<int>>()
                    .Where(uc => uc.UserId == user.Id)
                    .Select(uc => new IdentityUserClaim<int> { UserId = uc.UserId, ClaimType = uc.ClaimType, ClaimValue = uc.ClaimValue })
                    .ToListAsync(cancellationToken);
                var claimsToRemove = existingClaims.Intersect(userClaims);
                _dbContext.Set<IdentityUserClaim<int>>().RemoveRange(claimsToRemove);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }

        public override async Task AddLoginAsync(DomainUser user, UserLoginInfo login, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainUserStore.AddLoginAsync failed with {Code}: {Description}";
            try {
                var existingLogin = await _dbContext.Set<IdentityUserLogin<int>>()
                    .FirstOrDefaultAsync(ul => ul.UserId == user.Id
                        && ul.LoginProvider == login.LoginProvider
                        && ul.ProviderKey == login.ProviderKey, cancellationToken);
                if (existingLogin != default) {
                    var err = ErrorDescriber.LoginAlreadyAssociated();
                    _logger.LogWarning(baseErrMsg, err.Code, err.Description);
                } else {
                    _dbContext.Set<IdentityUserLogin<int>>().Add(new IdentityUserLogin<int> {
                        UserId = user.Id,
                        LoginProvider = login.LoginProvider,
                        ProviderKey = login.ProviderKey,
                        ProviderDisplayName = login.ProviderDisplayName
                    });
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }

        public override async Task RemoveLoginAsync(DomainUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainUserStore.RemoveLoginAsync failed with {Code}: {Description}";
            try {
                var existingLogin = await _dbContext.Set<IdentityUserLogin<int>>()
                    .FirstOrDefaultAsync(ul => ul.UserId == user.Id
                        && ul.LoginProvider == loginProvider
                        && ul.ProviderKey == providerKey, cancellationToken);
                if (existingLogin != default) {
                    _dbContext.Set<IdentityUserLogin<int>>().Remove(existingLogin);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }

        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(DomainUser user, CancellationToken cancellationToken = default) {
            return (await _dbContext.Set<IdentityUserLogin<int>>()
                .Where(ul => ul.UserId == user.Id)
                .ToListAsync(cancellationToken))
                .Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, ul.ProviderDisplayName))
                .ToList();
        }

        public override async Task<DomainUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) {
            return await _dbContext.Set<DomainUser>()
                .FirstOrDefaultAsync(u => EF.Functions.Like(u.NormalizedEmail, normalizedEmail));
        }

        public override async Task<IList<DomainUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) {
            return await _dbContext.Set<DomainUser>()
                .FromSqlInterpolated($@"
    SELECT u.* from AspNetUsers u
        WHERE EXISTS (
            SELECT 0 
                FROM AspNetUserClaims uc
                WHERE uc.UserId = u.Id
                    and uc.ClaimType = {claim.Type}
                    and uc.ClaimValue = {claim.Value}
        )").ToListAsync(cancellationToken);
        }

        protected override async Task<IdentityUserToken<int>> FindTokenAsync(DomainUser user, string loginProvider, string name, CancellationToken cancellationToken) {
            return await _dbContext.Set<IdentityUserToken<int>>().FirstOrDefaultAsync(
                ut => ut.UserId == user.Id && ut.LoginProvider == loginProvider && ut.Name == name, cancellationToken);
        }

        protected override async Task AddUserTokenAsync(IdentityUserToken<int> token) {
            string baseErrMsg = "DomainUserStore.AddUserTokenAsync failed with {Code}: {Description}";
            try {
                _dbContext.Set<IdentityUserToken<int>>().Add(token);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }

        protected override async Task RemoveUserTokenAsync(IdentityUserToken<int> token) {
            string baseErrMsg = "DomainUserStore.RemoveUserTokenAsync failed with {Code}: {Description}";
            try {
                _dbContext.Set<IdentityUserToken<int>>().Remove(token);
                await _dbContext.SaveChangesAsync();
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }
    }
}
