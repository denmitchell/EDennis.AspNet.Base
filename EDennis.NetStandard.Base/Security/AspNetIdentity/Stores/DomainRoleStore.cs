using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;


namespace EDennis.NetStandard.Base {

    public class DomainRoleStore : RoleStoreBase<DomainRole,int,IdentityUserRole<int>,IdentityRoleClaim<int>> {

        private readonly DomainIdentityDbContext _dbContext;
        private readonly IAppClaimEncoder _encoder;
        private readonly ILogger _logger;

        public override IQueryable<DomainRole> Roles => _dbContext.Set<DomainRole>().AsNoTracking();

        public DomainRoleStore(DomainIdentityDbContext dbContext, IAppClaimEncoder encoder,
            IdentityErrorDescriber errorDescriber, ILogger<DomainRoleStore> logger) : base (errorDescriber){
            _dbContext = dbContext;
            _encoder = encoder;
            _logger = logger;
        }


        private IdentityResult SyncNormalized(DomainRole role) {
            //if Application and Name are provided, encode NormalizedName from them 
            if (role.Application != default && role.Name != default) {
                role.NormalizedName = _encoder.Encode(new AppRole { Application = role.Application, RoleName = role.Name });
            //otherwise, if NormalizedName is provided, decode Application and Name from it
            } else if (role.NormalizedName != default) {
                var decoded = _encoder.Decode(role.NormalizedName);
                (role.Application, role.Name) = (decoded.Application, decoded.RoleName);
                if (role.Application == default)
                    return IdentityResult.Failed(ErrorDescriber.InvalidNormalizedName(role.NormalizedName));
            }
            return IdentityResult.Success;
        }

        private bool IsValidApplication(string application)
            => _dbContext.Set<DomainApplication>().Any(a => a.Name == application);

        public override async Task<IdentityResult> CreateAsync(DomainRole role, CancellationToken cancellationToken) {
            string baseErrMsg = "DomainRoleStore.CreateAsync failed with {Code}: {Description}";
            if (!IsValidApplication(role.Application)) {
                var err = ErrorDescriber.InvalidApplicationName(role.Application);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                return IdentityResult.Failed(err);
            }
            try {
                var syncResult = SyncNormalized(role);
                if (!syncResult.Succeeded) {
                    var err = syncResult.Errors.First();
                    _logger.LogError(baseErrMsg, err.Code, err.Description);
                    return syncResult;
                }
                _dbContext.Set<DomainRole>().Add(role);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                return IdentityResult.Failed(err);
            }
            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> UpdateAsync(DomainRole role, CancellationToken cancellationToken) {
            string baseErrMsg = "DomainRoleStore.UpdateAsync failed with {Code}: {Description}";
            if (!IsValidApplication(role.Application)) {
                var err = ErrorDescriber.InvalidApplicationName(role.Application);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                return IdentityResult.Failed(err);
            }
            try {
                var syncResult = SyncNormalized(role);
                if (!syncResult.Succeeded) {
                    var err = syncResult.Errors.First();
                    _logger.LogError(baseErrMsg, err.Code, err.Description);
                    return syncResult;
                }
                _dbContext.Set<DomainRole>().Update(role);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                return IdentityResult.Failed(err);
            }
            return IdentityResult.Success;
        }


        public override async Task<IdentityResult> DeleteAsync(DomainRole role, CancellationToken cancellationToken) {
            string baseErrMsg = "DomainRoleStore.DeleteAsync failed with {Code}: {Description}";
            try {
                _dbContext.Set<DomainRole>().Remove(role);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                return IdentityResult.Failed(err);
            }
            return IdentityResult.Success;
        }


        public override async Task<DomainRole> FindByIdAsync(string roleId, CancellationToken cancellationToken) {
            if (!int.TryParse(roleId, out int roleIdInt))
                return null;
            return await _dbContext.Set<DomainRole>().FirstOrDefaultAsync(e => e.Id == roleIdInt);
        }


        public override async Task<DomainRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) {
            return await _dbContext.Set<DomainRole>()
                .FirstOrDefaultAsync(e => EF.Functions.Like(e.NormalizedName, normalizedRoleName));
        }

        public override Task<string> GetNormalizedRoleNameAsync(DomainRole role, CancellationToken cancellationToken) {
            return Task.FromResult(role.NormalizedName ?? _encoder.Encode(new AppRole { Application = role.Application, RoleName = role.Name }));
        }

        public override Task<string> GetRoleIdAsync(DomainRole role, CancellationToken cancellationToken) {
            return Task.FromResult(role.Id.ToString());
        }

        public override Task<string> GetRoleNameAsync(DomainRole role, CancellationToken cancellationToken) {
            return Task.FromResult(role.Name);
        }

        public override Task SetNormalizedRoleNameAsync(DomainRole role, string normalizedName, CancellationToken cancellationToken) {
            role.NormalizedName = normalizedName;
            var syncResult = SyncNormalized(role);
            if (!syncResult.Succeeded)
                if (!syncResult.Succeeded) {
                    var err = syncResult.Errors.First();
                    var exception = new ArgumentException($"DomainRoleStore.UpdateAsync failed with {err.Code}: {err.Description}");
                    _logger.LogError(exception, exception.Message);
                    return Task.FromException(exception);
                }
            return Task.CompletedTask;
        }

        public override Task SetRoleNameAsync(DomainRole role, string roleName, CancellationToken cancellationToken) {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public override async Task<IList<Claim>> GetClaimsAsync(DomainRole role, CancellationToken cancellationToken = default) {
            return (await _dbContext.Set<IdentityRoleClaim<int>>()
                .Where(rc => rc.RoleId == role.Id)
                .ToListAsync(cancellationToken))
                .Select(rc=> _encoder.Encode(
                    new AppClaim { 
                        Application = role.Application, 
                        ClaimType = rc.ClaimType, 
                        ClaimValue = rc.ClaimValue  
                    }))
                .ToList();
        }

        public override async Task AddClaimAsync(DomainRole role, Claim claim, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainRoleStore.AddClaimAsync failed with {Code}: {Description}";
            //decode the claim just in case it has application name embedded in the claim type or claim value
            var appClaim = _encoder.Decode(claim);
            try {
                _dbContext.Set<IdentityRoleClaim<int>>().Add(
                    new IdentityRoleClaim<int> {
                        RoleId = role.Id,
                        ClaimType = appClaim.ClaimType,
                        ClaimValue = appClaim.ClaimValue
                    });
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }

        public override async Task RemoveClaimAsync(DomainRole role, Claim claim, CancellationToken cancellationToken = default) {
            string baseErrMsg = "DomainRoleStore.RemoveClaimAsync failed with {Code}: {Description}";
            //decode the claim just in case it has application name embedded in the claim type or claim value
            var appClaim = _encoder.Decode(claim);
            try {
                var claimEntity = _dbContext.Set<IdentityRoleClaim<int>>()
                .FirstOrDefault(rc => rc.RoleId == role.Id
                    && rc.ClaimType == appClaim.ClaimType
                    && rc.ClaimValue == appClaim.ClaimValue);
                _dbContext.Remove(claimEntity);
                await _dbContext.SaveChangesAsync(cancellationToken);
            } catch (DbUpdateException ex) {
                var err = ErrorDescriber.DbUpdateException(ex);
                _logger.LogError(baseErrMsg, err.Code, err.Description);
                throw new ApplicationException(err.Description, ex);
            }
        }
    }
}
