
using Microsoft.Extensions.Options;

namespace EDennis.NetStandard.Base {

    public class CentralAdminOptions {
        public string Email { get; set; }
    }

    public class CentralAdmin {

        public CentralAdmin(IOptionsMonitor<CentralAdminOptions> options) {
            Email = options.CurrentValue.Email;
        }

        public string Email { get; set; }
    }
}
