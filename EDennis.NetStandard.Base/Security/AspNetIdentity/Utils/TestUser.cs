using System;
using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class TestUser {
        public const bool EMAIL_CONFIRMED_DEFAULT = true;
        public const string PHONE_NUMBER_DEFAULT = "999.555.1212";
        public const bool PHONE_CONFIRMED_DEFAULT = true;
        public const bool ORGANIZATION_CONFIRMED_DEFAULT = true;
        public const bool ORGANIZATION_ADMIN_DEFAULT = false;
        public const bool SUPER_ADMIN_DEFAULT = false;
        public const bool LOCKED_OUT_DEFAULT = true;

        public const string PASSWORD_DEFAULT = "test";

        public string Email { get; set; }
        public bool EmailConfirmed { get; set; } = EMAIL_CONFIRMED_DEFAULT;
        public string PhoneNumber { get; set; } = PHONE_NUMBER_DEFAULT;
        public bool PhoneNumberConfirmed { get; set; } = PHONE_CONFIRMED_DEFAULT;
        public string Organization { get; set; }
        public bool OrganizationConfirmed { get; set; } = ORGANIZATION_CONFIRMED_DEFAULT;
        public bool OrganizationAdmin { get; set; } = ORGANIZATION_ADMIN_DEFAULT;
        public bool SuperAdmin { get; set; } = SUPER_ADMIN_DEFAULT;

        public bool LockedOut = LOCKED_OUT_DEFAULT;

        public List<string> Roles { get; set; }
        public Dictionary<string, List<string>> Claims { get; set; }

        public string PlainTextPassword { get; set; } = PASSWORD_DEFAULT;

    }
}
