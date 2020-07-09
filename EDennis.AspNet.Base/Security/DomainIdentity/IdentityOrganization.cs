using System;

namespace EDennis.AspNet.Base.Security {
    public class IdentityOrganization {
        public Guid Id { get; set; } = CombGuid.Create();
        public string Name { get; set; }
    }
}
