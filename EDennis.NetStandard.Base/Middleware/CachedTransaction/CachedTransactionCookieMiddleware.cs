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

    /// <summary>
    /// In combination with CachedTransactionMiddleware, used by a Child API,
    /// this middleware allows database transactions to remain uncommitted
    /// for multiple HTTP requests and to roll back after these requests
    /// (helpful for testing).
    /// This middleware gets or adds an "X-CachedTransaction" Cookie with a
    /// random guid.  Unlike CachedTransactionMiddleware, which manages
    /// a cache of SqlConnection and SqlTransaction objects for specific 
    /// DbContexts, CachedTransactionCookieMiddleware only creates cookies.
    /// This middleware is designed for applications that have indirect 
    /// access to a database via another API, which in turn uses the 
    /// CachedTransactionMiddleware.
    /// </summary>
    public class CachedTransactionCookieMiddleware {

        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<CachedTransactionOptions> _options;
        private string[] _enabledForClaims;

        public CachedTransactionCookieMiddleware(RequestDelegate next, 
            IOptionsMonitor<CachedTransactionOptions> options) {
            _next = next;
            _options = options;
            UpdateEnabledForClaims(options.CurrentValue.EnabledForClaims);
            _options.OnChange(e => UpdateEnabledForClaims(e.EnabledForClaims));
        }

        public async Task InvokeAsync(HttpContext context) {

            var claims = context.User?.Claims;

            if (claims != null && _enabledForClaims.Any(e => claims.Any(c => $"{c.Type}|{c.Value}" == e))) {
                var cookieValue = GetOrAddCookie(context, out bool cookieAdded);

                if (cookieAdded) {
                    context.Request.Headers.Add(CachedTransactionOptions.COOKIE_KEY, cookieValue);
                    context.Response.OnStarting(state => {
                        var httpContext = (HttpContext)state;
                        httpContext.Response.Cookies.Append(CachedTransactionOptions.COOKIE_KEY, cookieValue);
                        return Task.CompletedTask;
                    }, context);
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


    public static class IServiceCollectionExtensions_CachedTransactionCookieMiddleware {
        public static IServiceCollection AddCachedTransactionCookie(this IServiceCollection services, IConfiguration config,
            string configKey = CachedTransactionOptions.DEFAULT_CONFIG_KEY) {
            services.Configure<CachedTransactionOptions>(config.GetSection(configKey));
            return services;
        }
    }

    public static class IApplicationBuilderExtensions_CachedTransactionCookieMiddleware {
        public static IApplicationBuilder UseCachedTransactionCookie(this IApplicationBuilder app) {
            app.UseMiddleware<CachedTransactionCookieMiddleware>();
            return app;
        }

        public static IApplicationBuilder UseCachedTransactionCookieFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context =>
            {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseCachedTransactionCookie()
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
