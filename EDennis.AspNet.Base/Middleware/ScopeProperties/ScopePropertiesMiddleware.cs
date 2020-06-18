using EDennis.AspNet.Base.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Middleware {
    public class ScopePropertiesMiddleware {

        private readonly RequestDelegate _next;
        private readonly ScopePropertiesOptions _options;
        public ScopePropertiesMiddleware(RequestDelegate next, IOptionsMonitor<ScopePropertiesOptions> options) {
            _next = next;
            _options = options.CurrentValue;
        }

        public async Task InvokeAsync(HttpContext context, ScopeProperties scopeProperties) {

            if (context.Request.Path.Value.Contains("swagger"))
                await _next(context);
            else {

                var claims = context.User?.Claims;
                if (claims != default) {
                    var toInclude = 
                        _options.ClaimTypesToInclude 
                        .Union(_options.ClaimTypesToPropagate);

                    scopeProperties.Claims = context.User?.Claims
                        .Where(c => toInclude.Any(i => i == c.Type))
                        .ToArray();
                }

                var cookies = context.Request.Cookies;
                if (cookies.TryGetValue("TransactionScope", out string transactionScope))
                    scopeProperties.Add("TransactionScope", transactionScope);

                var headers = context.Request.Headers;
                if (headers.TryGetValue("Authorization", out StringValues authorization))
                    scopeProperties.Add("Authorization", authorization);
 
                await _next(context); 
            }

        }


    }
}
