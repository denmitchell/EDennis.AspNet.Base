using System.Linq;

namespace EDennis.AspNet.Base.Security.Extensions {
    public static class UserModelExtensions {
        public static UserEditModel ToEditModel(this DomainUser domainModel) {
            var editModel = new UserEditModel {
                Name = domainModel.UserName,
                AccessFailedCount = domainModel.AccessFailedCount,
                Email = domainModel.Email,
                EmailConfirmed = domainModel.EmailConfirmed,
                LockoutBegin = domainModel.LockoutBegin,
                LockoutEnd = domainModel.LockoutEnd,
                PhoneNumber = domainModel.PhoneNumber,
                PhoneNumberConfirmed = domainModel.PhoneNumberConfirmed,
                Password = domainModel.PasswordHash,
                SysStatus = domainModel.SysStatus,
                SysUser = domainModel.SysUser,
                TwoFactorEnabled = domainModel.TwoFactorEnabled,
                Properties = domainModel.Properties,
                Organization = domainModel.Organization.Name,
                Claims = domainModel.UserClaims.ToDictionary(),
                Roles = domainModel.UserRoles.Select(ur => ur.Role.Name).ToArray()                        
            };
            return editModel;
        }

    }
}
