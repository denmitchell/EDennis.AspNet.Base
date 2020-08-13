using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Scoped Lifetime
    /// </summary>
    public class ScopeProperties : ConcurrentDictionary<string, string> {
        HttpRequestMessage HttpRequestMessage { get; set; }


        public void CopyRequest(HttpRequest httpRequest) {
            HttpRequestMessage = new HttpRequestMessage();
            HttpRequestMessage
                .CopyMethod(httpRequest)
                .CopyHeaders(httpRequest)
                .CopyQueryString(httpRequest)
                .CopyCookies(httpRequest);
        }

    }
}
