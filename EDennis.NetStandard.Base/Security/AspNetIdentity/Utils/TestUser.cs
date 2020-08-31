using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class TestUser {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Organization { get; set; }
        public bool OrganizationAdmin { get; set; }
        public List<string> Roles { get; set; }
        public Dictionary<string, List<string>> Claims { get; set; }

        public string PlainTextPassword { get; set; }

    }
}
