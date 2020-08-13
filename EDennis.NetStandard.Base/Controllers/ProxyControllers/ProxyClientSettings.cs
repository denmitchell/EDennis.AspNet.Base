using System.Collections.Generic;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Note: string key should be the name of the ProxyController
    /// (minus "Controller")
    /// </summary>
    public class ProxyClients : Dictionary<string, ProxyClient> { }

    public class ProxyClient {
        public string TargetUrl { get; set; }
    }
}
