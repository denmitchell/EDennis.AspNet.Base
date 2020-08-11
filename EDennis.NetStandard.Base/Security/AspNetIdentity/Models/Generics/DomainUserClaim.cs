using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public abstract class DomainUserClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole> : IdentityUserClaim<int>, ITemporalEntity
        where TUser : DomainUser<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TOrganization : DomainOrganization<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserClaim : DomainUserClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserLogin : DomainUserLogin<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserToken : DomainUserToken<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRole : DomainRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TApplication : DomainApplication<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRoleClaim : DomainRoleClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserRole : DomainUserRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new() {

        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }

        public abstract void Patch(JsonElement jsonElement, ModelStateDictionary modelState);

        public abstract void Update(object updated);

        public TUser User { get; set; }


        public Dictionary<string, string[]> ToDictionary(IEnumerable<TUserClaim> claims) {
            return claims.GroupBy(c => new { c.ClaimType })
            .Select(g => new { Type = g.Key.ClaimType, Value = g.Select(i => i.ClaimValue).ToArray() })
                .ToDictionary(d => d.Type, d => d.Value);
        }

        public ICollection<TUserClaim> ToUserClaims(Dictionary<string, string[]> claims) {
            return claims.SelectMany(c => c.Value,
                (type, value) => new TUserClaim { ClaimType = type.Key, ClaimValue = value }).ToList();
        }


    }

}