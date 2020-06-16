using EDennis.AspNet.Base.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
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

                var user = context.User?.Claims?.FirstOrDefault(c => c.Type == "name")?.Value;
                if (user != default)
                    scopeProperties.Add("User", user);

                var cookies = context.Request.Cookies;
                if (cookies.ContainsKey("TransactionScope"))
                    scopeProperties.Add("TransactionScope", cookies["TransactionScope"]);

                var headers = context.Request.Headers;
                if (headers.ContainsKey("Authorization"))
                    scopeProperties.Add("Authorization", cookies["Authorization"]);
 
                await _next(context); 
            }

        }


    }
}
