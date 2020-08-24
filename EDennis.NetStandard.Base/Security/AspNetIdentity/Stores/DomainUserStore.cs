using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EDennis.NetStandard.Base {
    public class DomainUserStore : IUserStore<DomainUser> {

        private readonly DomainIdentityDbContext _dbContext;

        public DomainUserStore(DomainIdentityDbContext dbContext) {
            _dbContext = dbContext;
        }

        private void UpdateAppCalculatedColumns(DomainUser user) {
            user.NormalizedEmail = user.Email.ToUpper();
            user.NormalizedUserName = user.UserName.ToUpper();
        }

        private bool IsValidOrganization(string organization)
            => _dbContext.Set<DomainOrganization>().Any(a => a.Name == organization);

        public async Task<IdentityResult> CreateAsync(DomainUser user, CancellationToken cancellationToken) {
            if (!IsValidOrganization(user.Organization))
                return IdentityResult.Failed(new IdentityError {
                    Code = "ERR_CREATE_USER_ORG",
                    Description = $"An organization with the name {user.Organization} does not exist"
                });
            try {
                UpdateAppCalculatedColumns(user);
                _dbContext.Set<DomainUser>().Add(user);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                var msg = ex.Message + ": " + ex.InnerException?.Message ?? "";
                return IdentityResult.Failed(new IdentityError { Code = "ERR_CREATE_USER", Description = msg });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(DomainUser user, CancellationToken cancellationToken) {
            if (!IsValidOrganization(user.Organization))
                return IdentityResult.Failed(new IdentityError {
                    Code = "ERR_UPDATE_USER_ORG",
                    Description = $"An organization with the name {user.Organization} does not exist"
                });
            try {
                UpdateAppCalculatedColumns(user);
                _dbContext.Set<DomainUser>().Update(user);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                var msg = ex.Message + ": " + ex.InnerException?.Message ?? "";
                return IdentityResult.Failed(new IdentityError { Code = "ERR_UPDATE_USER", Description = msg });
            }
            return IdentityResult.Success;
        }



        public async Task<IdentityResult> DeleteAsync(DomainUser user, CancellationToken cancellationToken) {
            try {
                _dbContext.Set<DomainUser>().Remove(user);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                var msg = ex.Message + ": " + ex.InnerException?.Message ?? "";
                return IdentityResult.Failed(new IdentityError { Code = "ERR_DELETE_USER", Description = msg });
            }
            return IdentityResult.Success;
        }

        public void Dispose() {
            throw new NotImplementedException();
        }

        public async Task<DomainUser> FindByIdAsync(string userId, CancellationToken cancellationToken) {
            if (!int.TryParse(userId, out int userIdInt))
                return null;
            return await _dbContext.Set<DomainUser>().FirstOrDefaultAsync(e => e.Id == userIdInt);
        }

        public async Task<DomainUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) {
            return await _dbContext.Set<DomainUser>().FirstOrDefaultAsync(e => e.NormalizedUserName == normalizedUserName.ToUpper());
        }

        public Task<string> GetNormalizedUserNameAsync(DomainUser user, CancellationToken cancellationToken) {
            return Task.FromResult(user.NormalizedUserName ?? user.UserName.ToUpper());
        }

        public Task<string> GetUserIdAsync(DomainUser user, CancellationToken cancellationToken) {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(DomainUser user, CancellationToken cancellationToken) {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(DomainUser user, string normalizedName, CancellationToken cancellationToken) {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetUserNameAsync(DomainUser user, string userName, CancellationToken cancellationToken) {
            user.UserName = userName;
            return Task.CompletedTask;
        }

    }
}
