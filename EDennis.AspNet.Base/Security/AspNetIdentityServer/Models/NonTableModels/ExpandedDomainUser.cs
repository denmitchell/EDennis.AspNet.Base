using System;
using System.Collections.Generic;

namespace EDennis.AspNet.Base.Security {
    public class ExpandedDomainUser : DomainUser {
        public string OrganizationName { get; set; }
        public Dictionary<string,List<string>> RolesDictionary { get; set; }
        public Dictionary<string,List<string>> ClaimsDictionary { get; set; }

    }
}
