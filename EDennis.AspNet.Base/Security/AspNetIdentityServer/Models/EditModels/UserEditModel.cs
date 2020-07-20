using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EDennis.AspNet.Base.Security {
    public class UserEditModel : BaseEditModel {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutBegin { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public string Organization { get; set; }
        public Dictionary<string, string[]> Claims { get; set; }
        public IEnumerable<string> Roles { get; set; }

    }
}
