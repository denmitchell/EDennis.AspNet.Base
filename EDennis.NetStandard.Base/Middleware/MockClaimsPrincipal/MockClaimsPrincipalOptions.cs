using System.Collections.Generic;

namespace EDennis.NetStandard.Base {
    public class MockClaimsPrincipalOptions {
        public const string SELECTED_MOCK_CLAIMS_PRINCIPAL_ARGUMENT = "mcp";
        public string Selected { get; set; }
        public Dictionary<string, Dictionary<string, string[]>> Pool { get; set; }
    }
}
