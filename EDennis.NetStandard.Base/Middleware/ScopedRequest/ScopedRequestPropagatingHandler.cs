using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    public class ScopedRequestPropagatingHandler: HttpClientHandler {

        private readonly ScopedRequestMessage _scopedRequestMessage;

        public ScopedRequestPropagatingHandler(ScopedRequestMessage scopedRequestMessage) {
            _scopedRequestMessage = scopedRequestMessage;
            CookieContainer = new CookieContainer();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            
            if(_scopedRequestMessage.TryGetCookie(CachedTransactionOptions.COOKIE_KEY, out string transactionScope))
                CookieContainer.Add(new Cookie(CachedTransactionOptions.COOKIE_KEY, transactionScope));

            if (_scopedRequestMessage.TryGetHeader(HeaderToClaimsOptions.HEADER_KEY, out string claimsHeader))
                request.Headers.Add(HeaderToClaimsOptions.HEADER_KEY, claimsHeader);


            return await base.SendAsync(request, cancellationToken);
            
        }


    }
}
