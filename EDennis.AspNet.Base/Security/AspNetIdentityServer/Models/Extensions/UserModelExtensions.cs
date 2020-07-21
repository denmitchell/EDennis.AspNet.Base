using IdentityModel;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security.AspNetIdentityServer.Models.Extensions {
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

        public static DomainUser ToDomainModel(this UserEditModel editModel, DomainIdentityDbContext dbContext) {
            var existing = dbContext.Set<DomainUser>().FirstOrDefault(u => u.UserName == editModel.Name);

            var domainModel = new DomainUser {
                Id = existing?.Id ?? default,
                UserName = editModel.Name,
                AccessFailedCount = editModel.AccessFailedCount,
                Email = editModel.Email,
                EmailConfirmed = editModel.EmailConfirmed,
                LockoutBegin = editModel.LockoutBegin,
                LockoutEnd = editModel.LockoutEnd,
                PhoneNumber = editModel.PhoneNumber,
                PhoneNumberConfirmed = editModel.PhoneNumberConfirmed,
                PasswordHash = (editModel.Password.Length == DomainUser.SHA256_LENGTH || editModel.Password.Length == DomainUser.SHA512_LENGTH) ? editModel.Password : editModel.Password.ToSha256(),
                SysStatus = editModel.SysStatus,
                SysUser = editModel.SysUser,
                TwoFactorEnabled = editModel.TwoFactorEnabled,
                Properties = editModel.Properties,
                Organization = dbContext.Set<DomainOrganization>()
                    .FirstOrDefault(o=>o.Name == editModel.Organization),
                UserClaims = editModel.Claims.ToICollection(),
                UserRoles = dbContext.Set<DomainUserRole>().FromSqlInterpolated($@"
SELECT ur.*
    FROM DomainUserRoles ur
    INNER JOIN DomainUsers u
        ON ur.UserId = u.Id
    INNER JOIN DomainRoles r
        ON ur.RoleId = r.Id
    WHERE
        u.UserName = {editModel.Name}
        AND r.Name in ({"'" + string.Join("','",editModel.Roles) + "'"})
    ").ToList(),
            };

            return domainModel;

        }



    }
}
