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
        public ScopePropertiesMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ScopeProperties scopeProperties) {

            if (context.Request.Path.Value.Contains("swagger"))
                await _next(context);
            else {

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
