using System.Collections.Generic;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Note: string key should be the name of the ApiClient class
    /// </summary>
    public class ApiClients : Dictionary<string, ApiClient> { }

    public class ApiClient {
        public string TargetUrl { get; set; }
        public string[] OtherAuthorizedUrls { get; set; }
        public string[] Scopes { get; set; }
    }
}
