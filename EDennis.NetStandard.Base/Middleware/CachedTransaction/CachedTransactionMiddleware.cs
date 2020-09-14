using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly IOptionsMonitor<CachedTransactionOptions> _options;
        private readonly ILogger<CachedTransactionMiddleware<TContext>> _logger;
        private string[] _enabledForClaims;

        public CachedTransactionMiddleware(RequestDelegate next, TransactionCache<TContext> cache,
            IOptionsMonitor<CachedTransactionOptions> options, ILogger<CachedTransactionMiddleware<TContext>> logger) {
            _next = next;
            _cache = cache;
            _options = options;
            _logger = logger;
            _logger.LogDebug("CachedTransactionMiddleware for {TContextName} constructed with {@CachedTransactionOptions}", typeof(TContext).Name, options.CurrentValue);
            UpdateEnabledForClaims(options.CurrentValue.EnabledForClaims);
            _options.OnChange(e => UpdateEnabledForClaims(e.EnabledForClaims));
        }

        public async Task InvokeAsync(HttpContext context) {

            var claims = context.User?.Claims;

            if (claims != null && _enabledForClaims.Any(e => claims.Any(c => $"{c.Type}|{c.Value}" == e))) {
                var cookieValue = GetOrAddCookie(context, out bool cookieAdded);

                using (_logger.BeginScope("CachedTransactionMiddleware for {TContextName} executing for user with Claims: {@Claims}.", typeof(TContext).Name, context.User.Claims)) {

                    var dbContextProvider = context.RequestServices.GetRequiredService<DbContextProvider<TContext>>();
                    _logger.LogTrace("Replacing DbContext.", typeof(TContext).Name);
                    _cache.ReplaceDbContext(Guid.Parse(cookieValue), dbContextProvider);

                    if (cookieAdded) {
                        context.Request.Headers.Add(CachedTransactionOptions.COOKIE_KEY, cookieValue);
                        context.Response.OnStarting(state => {
                            var httpContext = (HttpContext)state;
                            _logger.LogDebug("Setting cookie ({CookieKey}, {CookieValue}).", typeof(TContext).Name, CachedTransactionOptions.COOKIE_KEY, cookieValue);
                            httpContext.Response.Cookies.Append(CachedTransactionOptions.COOKIE_KEY, cookieValue);
                            return Task.CompletedTask;
                        }, context);
                    }

                    if (context.Request.Path.Value.EndsWith(CachedTransactionOptions.ROLLBACK_PATH)) {
                        _logger.LogDebug("Initiating rollback.", typeof(TContext).Name);
                        await _cache.RollbackAsync(Guid.Parse(cookieValue));
                        return;
                    } else if (context.Request.Path.Value.EndsWith(CachedTransactionOptions.COMMIT_PATH)) {
                        _logger.LogDebug("Initiating commit.", typeof(TContext).Name);
                        await _cache.CommitAsync(Guid.Parse(cookieValue));
                        return;
                    }
                }

                await _next(context);

            } else
                await _next(context); 

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


    public static class IServiceCollectionExtensions_CachedTransactionMiddleware {
        public static IServiceCollection AddCachedTransaction(this IServiceCollection services, IConfiguration config,
            string configKey = CachedTransactionOptions.DEFAULT_CONFIG_KEY) {
            services.Configure<CachedTransactionOptions>(config.GetSection(configKey));
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
