using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class MockUserMiddleware {

        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<MockUserOptions> _options;

        public MockUserMiddleware(RequestDelegate next,
            IOptionsMonitor<MockUserOptions> options) {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context) {

            if (!_options.CurrentValue.Enabled)
                await _next(context);
            else {
                var claims = _options.CurrentValue.Claims.ToClaimEnumerable();
                context.User = new ClaimsPrincipal(new ClaimsIdentity(claims,"mockAuth"));
                 
                await _next(context); 
            }

        }

    }

    public static class IServiceCollectionExtensions_MockUserMiddleware {
        public static IServiceCollection AddMockUser(this IServiceCollection services, IConfiguration config) {
            services.Configure<MockUserOptions>(config.GetSection("Security:MockUser"));
            return services;
        }
    }

    public static class IApplicationBuilderExtensions_MockUserMiddleware {
        public static IApplicationBuilder UseMockUser(this IApplicationBuilder app) {
            app.UseMiddleware<MockUserMiddleware>();
            return app;
        }


        public static IApplicationBuilder UseMockUserFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context =>
            {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseMockUser()
            );
            return app;
        }

        public static IApplicationBuilder UseMockUserWhen(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate) {
            app.UseWhen(predicate, app => app.UseMockUser());
            return app;
        }


    }

}


