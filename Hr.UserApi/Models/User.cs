using EDennis.AspNet.Base;
using EDennis.AspNetIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Admin.UserApi.Models {
    public class User : IdentityUser, ICrudEntity {

        public List<UserClaim> Claims { get; set; }
        public List<UserRole> Roles { get; set; }


        [NotMapped]
        public SysStatus SysStatus { get; set; } = SysStatus.Normal;

        [NotMapped]
        public string SysUser { get; set; } = "system";


        public void Patch(JsonElement jsonElement) {
            bool needsSecurityStamp = false;

            if (jsonElement.TryGetInt32("AccessFailedCount", out int accessFailedCount))
                AccessFailedCount = accessFailedCount;

            else if (jsonElement.TryGetBoolean("LockoutEnabled", out bool lockoutEnabled))
                LockoutEnabled = lockoutEnabled;
            else if (jsonElement.TryGetDateTime("LockoutEnd", out DateTime lockoutEnd))
                LockoutEnd = lockoutEnd;

            else if (jsonElement.TryGetString("UserName", out string userName)) {
                UserName = userName;
                NormalizedUserName = userName.ToUpper();
                needsSecurityStamp = true;
            } else if (jsonElement.TryGetString("PasswordHash", out string passwordHash)) {
                PasswordHash = passwordHash;
                needsSecurityStamp = true;
            } else if (jsonElement.TryGetString("Email", out string email)) {
                Email = email;
                NormalizedEmail = email.ToUpper();
            } else if (jsonElement.TryGetBoolean("EmailConfirmed", out bool emailConfirmed))
                EmailConfirmed = emailConfirmed;

            else if (jsonElement.TryGetString("PhoneNumber", out string phone))
                PhoneNumber = phone;
            else if (jsonElement.TryGetBoolean("PhoneNumberConfirmed", out bool phoneConfirmed))
                PhoneNumberConfirmed = phoneConfirmed;

            else if (jsonElement.TryGetBoolean("TwoFactorEnabled", out bool twoFactorEnabled))
                TwoFactorEnabled = twoFactorEnabled;


            if (needsSecurityStamp)
                SecurityStamp = Guid.NewGuid().ToString();

            ConcurrencyStamp = Guid.NewGuid().ToString();

        }



        public void Update(object updated) {

            var user = updated as User;
            bool needsSecurityStamp = false;

            AccessFailedCount = user.AccessFailedCount;

            LockoutEnabled = user.LockoutEnabled;
            LockoutEnd = user.LockoutEnd;

            if (UserName != user.UserName) {
                UserName = user.UserName;
                NormalizedUserName = user.UserName.ToUpper();
                needsSecurityStamp = true;
            }

            if (PasswordHash != user.PasswordHash) {
                PasswordHash = user.PasswordHash;
                needsSecurityStamp = true;
            }

            Email = user.Email;
            NormalizedEmail = user.Email.ToUpper();
            EmailConfirmed = user.EmailConfirmed;

            PhoneNumber = user.PhoneNumber;
            PhoneNumberConfirmed = user.PhoneNumberConfirmed;

            TwoFactorEnabled = user.TwoFactorEnabled;

            if (needsSecurityStamp)
                SecurityStamp = Guid.NewGuid().ToString();

            ConcurrencyStamp = Guid.NewGuid().ToString();
        }

    }
}
