using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class MockClaimsPrincipalOptions : Dictionary<string,Dictionary<string,string[]>> {
        public const string SELECTED_MOCK_CLAIMS_PRINCIPAL_KEY = "mcp";
    }
}
