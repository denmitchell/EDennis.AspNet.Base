using EDennis.AspNet.Base.Middleware;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Handlers {

    public class ScopePropertiesPropagatingHandler: HttpClientHandler {

        private readonly ScopeProperties _scopeProperties;
        private readonly ScopePropertiesOptions _options;

        public ScopePropertiesPropagatingHandler(ScopeProperties scopeProperties, IOptionsMonitor<ScopePropertiesOptions> options) {
            _scopeProperties = scopeProperties;
            _options = options.CurrentValue;
            CookieContainer = new CookieContainer();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {

            if(_scopeProperties.TryGetValue("TransactionScope", out string transactionScope))
                CookieContainer.Add(new Cookie("TransactionScope", transactionScope));

            if (_scopeProperties.TryGetValue("Authorization", out string authorization))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",authorization.ToString().Replace("Bearer ", ""));
                CookieContainer.Add(new Cookie("TransactionScope", transactionScope));

            if (_scopeProperties.TryGetValue("X-User", out string user))
                request.Headers.Add("X-User", user);

            return await base.SendAsync(request, cancellationToken);
            
        }


    }
}
