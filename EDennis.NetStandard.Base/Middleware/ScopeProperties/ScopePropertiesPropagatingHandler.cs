using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

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
