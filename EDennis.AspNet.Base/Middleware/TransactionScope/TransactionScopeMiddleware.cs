using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace EDennis.AspNet.Base.Middleware {
    public class TransactionScopeMiddleware {

        private readonly RequestDelegate _next;
        private readonly TransactionScopes _transactionScopes;
        private IOptionsMonitor<TransactionScopeOptions> _options;
        private string[] _enabledForClaims;

        public const string COOKIE_KEY = "TransactionScope";

        public TransactionScopeMiddleware(RequestDelegate next, TransactionScopes transactionScopes,
            IOptionsMonitor<TransactionScopeOptions> options) {
            _next = next;
            _transactionScopes = transactionScopes;
            _options = options;
            UpdateEnabledForClaims(options.CurrentValue.EnabledForClaims);
            _options.OnChange(e => UpdateEnabledForClaims(e.EnabledForClaims));
        }

        public async Task InvokeAsync(HttpContext context) {

            if (context.Request.Path.Value.Contains("swagger"))
                await _next(context);
            else {
                var claims = context.User?.Claims;
                if (claims != null && _enabledForClaims.Any(e => claims.Any(c => $"{c.Type}|{c.Value}" == e))) {
                    var cookieValue = GetOrAddCookie(context, out bool cookieAdded);
                    using var scope = _transactionScopes.GetOrAdd(cookieValue,
                            new TransactionScope(TransactionScopeOption.Required,
                                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }));
                    await _next(context); // in transaction scope
                    if (cookieAdded)
                        context.Response.Cookies.Append(COOKIE_KEY, cookieValue);
                } else
                    await _next(context); // not in transaction scope
            }

        }

        private string GetOrAddCookie(HttpContext context, out bool added) {
            if (context.Request.Cookies.TryGetValue(COOKIE_KEY, out string cookieValue)) {
                added = false;
                return cookieValue;
            } else {
                added = true;
                return Guid.NewGuid().ToString();
            }
        }

        private void UpdateEnabledForClaims(Dictionary<string, string[]> eClaims) {
            _enabledForClaims = eClaims.Keys
                .SelectMany(k => eClaims[k]
                .Select(v => $"{k}|{v}"))
                .ToArray();
        }

    }


    public static class IServiceCollectionExtensions_TransactionScopeMiddleware {
        public static IServiceCollection AddTransactionScope(this IServiceCollection services, IConfiguration config) {
            services.Configure<TransactionScopeOptions>(config.GetSection("TransactionScope"));
            return services;
        }
    }

    public static class IApplicationBuilderExtensions_TransactionScopeMiddleware {
        public static IApplicationBuilder UseTransactionScope(this IApplicationBuilder app) {
            app.UseMiddleware<TransactionScopeMiddleware>();
            return app;
        }
    }



}
