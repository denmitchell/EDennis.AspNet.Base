using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public static class DomainClaimTypes {
        public const string Organization = "organization";
        public const string OrganizationConfirmed = "organization_confirmed";
        public const string OrganizationAdminFor = "organization_admin_for";
        public const string SuperAdmin = "super_admin";
        public const string Locked = "locked";
        public const string ApplicationRolePrefix = "role:";
        public static string ApplicationRole(string appName) => $"{ApplicationRolePrefix}:{appName}";
    }

    public static class DomainClaimValues {
        public const string Admin = "admin";
    }
}
