using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class ScopePropertiesMiddleware {

        private readonly RequestDelegate _next;
        public ScopePropertiesMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ScopeProperties scopeProperties) {

            if (context.Request.Path.Value.Contains("swagger"))
                await _next(context);
            else {

                //only relevant during testing
                var cookies = context.Request.Cookies;
                if (cookies.TryGetValue(CachedTransactionOptions.COOKIE_KEY, out string transactionScope))
                    scopeProperties.Add(CachedTransactionOptions.COOKIE_KEY, transactionScope);

                //relevant during testing and production
                var headers = context.Request.Headers;
                if (headers.TryGetValue(PassthroughClaimsOptions.CLAIMS_HEADER, out StringValues passthroughClaims))
                    scopeProperties.Add(PassthroughClaimsOptions.CLAIMS_HEADER, passthroughClaims.ToString());
 
                await _next(context); 
            }

        }


    }
}
