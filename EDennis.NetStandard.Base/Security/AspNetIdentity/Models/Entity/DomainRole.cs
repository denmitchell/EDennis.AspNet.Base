using Microsoft.AspNetCore.Identity;

namespace EDennis.NetStandard.Base {
    public class DomainRole : IdentityRole<int> { 
        public string Application { get; set; }

    }
}
