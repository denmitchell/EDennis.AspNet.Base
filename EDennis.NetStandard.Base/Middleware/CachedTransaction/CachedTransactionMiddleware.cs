using EDennis.NetStandard.Base.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace EDennis.NetStandard.Base.Middleware {
    public class CachedTransactionMiddleware<TContext>
        where TContext : DbContext {

        private readonly RequestDelegate _next;
        private readonly TransactionCache<TContext> _cache;
        private IOptionsMonitor<CachedTransactionOptions> _options;
        private string[] _enabledForClaims;

        public const string COOKIE_KEY = "CachedTransaction";
        public const string ROLLBACK_PATH = "/rollback-cached-transaction";
        public const string COMMIT_PATH = "/commit-cached-transaction";

        public CachedTransactionMiddleware(RequestDelegate next, TransactionCache<TContext> cache,
            IOptionsMonitor<CachedTransactionOptions> options) {
            _next = next;
            _cache = cache;
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

                    var dbContextProvider = context.RequestServices.GetRequiredService<DbContextProvider<TContext>>();
                    _cache.ReplaceDbContext(Guid.Parse(cookieValue), dbContextProvider);

                    if (cookieAdded)
                        context.Response.OnStarting(state => {
                            var httpContext = (HttpContext)state;
                            httpContext.Response.Cookies.Append(COOKIE_KEY, cookieValue);
                            return Task.CompletedTask;
                        }, context);

                    if (context.Request.Path.Value.EndsWith(ROLLBACK_PATH)) {
                        await _cache.RollbackAsync(Guid.Parse(cookieValue));
                        return;
                    } else if (context.Request.Path.Value.EndsWith(COMMIT_PATH)) { 
                        await _cache.CommitAsync(Guid.Parse(cookieValue));
                        return;
                    }

                    await _next(context);

                } else
                    await _next(context); 
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
            services.Configure<CachedTransactionOptions>(config.GetSection("CachedTransaction"));
            return services;
        }
    }

    public static class IApplicationBuilderExtensions_TransactionScopeMiddleware {
        public static IApplicationBuilder UseTransactionScope<TContext>(this IApplicationBuilder app)
            where TContext: DbContext {
            app.UseMiddleware<CachedTransactionMiddleware<TContext>>();
            return app;
        }
    }



}
