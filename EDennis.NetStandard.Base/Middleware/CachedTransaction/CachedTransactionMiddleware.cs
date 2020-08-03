using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class CachedTransactionMiddleware<TContext>
        where TContext : DbContext {

        private readonly RequestDelegate _next;
        private readonly TransactionCache<TContext> _cache;
        private IOptionsMonitor<CachedTransactionOptions> _options;
        private string[] _enabledForClaims;

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
                            httpContext.Response.Cookies.Append(CachedTransactionOptions.COOKIE_KEY, cookieValue);
                            return Task.CompletedTask;
                        }, context);

                    if (context.Request.Path.Value.EndsWith(CachedTransactionOptions.ROLLBACK_PATH)) {
                        await _cache.RollbackAsync(Guid.Parse(cookieValue));
                        return;
                    } else if (context.Request.Path.Value.EndsWith(CachedTransactionOptions.COMMIT_PATH)) { 
                        await _cache.CommitAsync(Guid.Parse(cookieValue));
                        return;
                    }

                    await _next(context);

                } else
                    await _next(context); 
            }

        }

        private string GetOrAddCookie(HttpContext context, out bool added) {
            if (context.Request.Cookies.TryGetValue(CachedTransactionOptions.COOKIE_KEY, out string cookieValue)) {
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

    public static class IApplicationBuilderExtensions_CachedTransactionMiddleware {
        public static IApplicationBuilder UseCachedTransaction<TContext>(this IApplicationBuilder app)
            where TContext: DbContext {
            app.UseMiddleware<CachedTransactionMiddleware<TContext>>();
            return app;
        }

        public static IApplicationBuilder UseCachedTransactionFor<TContext>(this IApplicationBuilder app,
            params string[] startsWithSegments)
            where TContext : DbContext {
            app.UseWhen(context =>
            {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseCachedTransaction<TContext>()
            );
            return app;
        }

        public static IApplicationBuilder UseCachedTransactionWhen<TContext>(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate)
            where TContext : DbContext {
                app.UseWhen(predicate, app => app.UseCachedTransaction<TContext>());
            return app;
        }


    }



}
