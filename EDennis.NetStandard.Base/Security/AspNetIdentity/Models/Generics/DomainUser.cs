
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public abstract class DomainUser<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole> : IdentityUser<Guid>, ITemporalEntity
        where TUser : DomainUser<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TOrganization : DomainOrganization<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserClaim : DomainUserClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserLogin : DomainUserLogin<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserToken : DomainUserToken<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRole : DomainRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TApplication : DomainApplication<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRoleClaim : DomainRoleClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserRole : DomainUserRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new() {

        public const int SHA256_LENGTH = 64; //"a8a2f6ebe286697c527eb35a58b5539532e9b3ae3b64d4eb0a46fb657b41562c";
        public const int SHA512_LENGTH = 128; //"f3bf9aa70169e4ab5339f20758986538fe6c96d7be3d184a036cde8161105fcf53516428fa096ac56247bb88085b0587d5ec8e56a6807b1af351305b2103d74b";


        public override bool LockoutEnabled { get; set; }

        public DateTimeOffset? _lockoutBegin;
        public DateTimeOffset? _lockoutEnd;


        public DateTimeOffset? LockoutBegin {
            get => _lockoutBegin;
            set {
                _lockoutBegin = value;
                if (_lockoutBegin <= DateTime.Now && _lockoutEnd > DateTime.Now)
                    LockoutEnabled = true;
                else
                    LockoutEnabled = false;
            }
        }

        public override DateTimeOffset? LockoutEnd {
            get => _lockoutEnd;
            set {
                _lockoutEnd = value;
                if (_lockoutBegin <= DateTime.Now && _lockoutEnd > DateTime.Now)
                    LockoutEnabled = true;
                else
                    LockoutEnabled = false;
            }            
        }

        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }

        public Guid OrganizationId { get; set; }
        public TOrganization Organization { get; set; }

        public ICollection<TUserClaim> Claims { get; set; }
        public ICollection<TUserLogin> Logins { get; set; }
        public ICollection<TUserToken> Tokens { get; set; }
        public ICollection<TUserRole> UserRoles { get; set; }


        public abstract void Patch(JsonElement jsonElement, ModelStateDictionary modelState);

        public abstract void Update(object updated);


    }
}
