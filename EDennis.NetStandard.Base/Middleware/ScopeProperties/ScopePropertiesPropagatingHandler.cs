using EDennis.NetStandard.Base.Middleware;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.Handlers {

    public class ScopePropertiesPropagatingHandler: HttpClientHandler {

        private readonly ScopeProperties _scopeProperties;

        public ScopePropertiesPropagatingHandler(ScopeProperties scopeProperties) {
            _scopeProperties = scopeProperties;
            CookieContainer = new CookieContainer();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {

            if(_scopeProperties.TryGetValue("TransactionScope", out string transactionScope))
                CookieContainer.Add(new Cookie("TransactionScope", transactionScope));

            if (_scopeProperties.TryGetValue("Authorization", out string authorization))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",authorization.ToString().Replace("Bearer ", ""));

            return await base.SendAsync(request, cancellationToken);
            
        }


    }
}
