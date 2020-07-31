using EDennis.NetStandard.Base.Middleware;
using EDennis.NetStandard.Base.Web;
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

            if(_scopeProperties.TryGetValue(CachedTransactionOptions.COOKIE_KEY, out string transactionScope))
                CookieContainer.Add(new Cookie(CachedTransactionOptions.COOKIE_KEY, transactionScope));

            if (_scopeProperties.TryGetValue(PassthroughClaimsOptions.CLAIMS_HEADER, out string claimsHeader))
                request.Headers.Add(PassthroughClaimsOptions.CLAIMS_HEADER, claimsHeader);


            return await base.SendAsync(request, cancellationToken);
            
        }


    }
}
