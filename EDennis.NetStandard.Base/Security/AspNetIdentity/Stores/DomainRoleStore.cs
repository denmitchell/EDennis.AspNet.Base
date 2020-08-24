using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace EDennis.NetStandard.Base {
    public class DomainRoleStore : IRoleStore<DomainRole> {

        private readonly DomainIdentityDbContext _dbContext;
        private readonly IAppClaimEncoder _encoder;

        public DomainRoleStore(DomainIdentityDbContext dbContext, IAppClaimEncoder encoder) {
            _dbContext = dbContext;
            _encoder = encoder;
        }

        private void UpdateAppCalculatedColumns(DomainRole role) {
            role.Name = _encoder.Encode(new AppRole { Application = role.Application, RoleNomen = role.Nomen });
            role.NormalizedName = role.Name.ToUpper();
        }

        private bool IsValidApplication(string application)
            => _dbContext.Set<DomainApplication>().Any(a => a.Name == application);

        public async Task<IdentityResult> CreateAsync(DomainRole role, CancellationToken cancellationToken) {
            if(!IsValidApplication(role.Application))
                return IdentityResult.Failed(new IdentityError {
                    Code = "ERR_CREATE_ROLE_APP",
                    Description = $"An application with the name {role.Application} does not exist"
                });
            try {
                UpdateAppCalculatedColumns(role);
                _dbContext.Set<DomainRole>().Add(role);
                await _dbContext.SaveChangesAsync();
            } catch(Exception ex) {
                var msg = ex.Message + ": " + ex.InnerException?.Message ?? "";
                return IdentityResult.Failed(new IdentityError { Code = "ERR_CREATE_ROLE", Description = msg });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(DomainRole role, CancellationToken cancellationToken) {
            if (!IsValidApplication(role.Application))
                return IdentityResult.Failed(new IdentityError {
                    Code = "ERR_UPDATE_ROLE_APP",
                    Description = $"An application with the name {role.Application} does not exist"
                });
            try {
                UpdateAppCalculatedColumns(role);
                _dbContext.Set<DomainRole>().Update(role);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                var msg = ex.Message + ": " + ex.InnerException?.Message ?? "";
                return IdentityResult.Failed(new IdentityError { Code = "ERR_UPDATE_ROLE", Description = msg });
            }
            return IdentityResult.Success;
        }


        public async Task<IdentityResult> DeleteAsync(DomainRole role, CancellationToken cancellationToken) {
            try {
                _dbContext.Set<DomainRole>().Remove(role);
                await _dbContext.SaveChangesAsync();
            } catch (Exception ex) {
                var msg = ex.Message + ": " + ex.InnerException?.Message ?? "";
                return IdentityResult.Failed(new IdentityError { Code = "ERR_DELETE_ROLE", Description = msg });
            }
            return IdentityResult.Success;
        }


        public async Task<DomainRole> FindByIdAsync(string roleId, CancellationToken cancellationToken) {
            if (!int.TryParse(roleId, out int roleIdInt))
                return null;
            return await _dbContext.Set<DomainRole>().FirstOrDefaultAsync(e => e.Id == roleIdInt);
        }

        public async Task<DomainRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) {
            return await _dbContext.Set<DomainRole>().FirstOrDefaultAsync(e => e.NormalizedName == normalizedRoleName);
        }

        public Task<string> GetNormalizedRoleNameAsync(DomainRole role, CancellationToken cancellationToken) {
            return Task.FromResult(role.NormalizedName ?? role.Name.ToUpper());
        }

        public Task<string> GetRoleIdAsync(DomainRole role, CancellationToken cancellationToken) {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(DomainRole role, CancellationToken cancellationToken) {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(DomainRole role, string normalizedName, CancellationToken cancellationToken) {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetRoleNameAsync(DomainRole role, string roleName, CancellationToken cancellationToken) {
            var appRole = _encoder.Decode(roleName);
            role.Application = appRole.Application;

            if (!IsValidApplication(role.Application))
                throw new ArgumentException($"An application with the name {role.Application} does not exist");

            role.Nomen = appRole.RoleNomen;
            role.Name = roleName;
            role.NormalizedName = roleName.ToUpper();
            return Task.CompletedTask; 
        }



        public void Dispose() {
            throw new NotImplementedException();
        }

    }
}
