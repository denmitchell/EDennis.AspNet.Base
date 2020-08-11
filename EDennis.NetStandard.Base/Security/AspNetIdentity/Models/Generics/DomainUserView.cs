
using Microsoft.AspNetCore.Identity;
using System;

namespace EDennis.NetStandard.Base {
    public class DomainUserView<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole> : IdentityUser<int>
        where TUser : DomainUser<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TOrganization : DomainOrganization<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserClaim : DomainUserClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserLogin : DomainUserLogin<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserToken : DomainUserToken<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRole : DomainRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TApplication : DomainApplication<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TRoleClaim : DomainRoleClaim<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new()
        where TUserRole : DomainUserRole<TUser, TOrganization, TUserClaim, TUserLogin, TUserToken, TRole, TApplication, TRoleClaim, TUserRole>, new() {

        public string OrganizationName { get; set; }
        public DateTimeOffset? LockoutBegin { get; set; }
        public string RolesDictionary { get; set; }
        public string ClaimsDictionary { get; set; }

    }
}
