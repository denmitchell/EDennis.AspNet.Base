using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public static class DomainClaimTypes {
        public const string Organization = "organization";
        public const string OrganizationConfirmed = "organization_confirmed";
        public const string OrganizationAdmin = "organization_admin";
        public const string Locked = "locked";
    }
}
