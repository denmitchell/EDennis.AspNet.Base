using EDennis.AspNet.Base.Extensions;
using EDennis.AspNet.Base.Middleware.MockUser;
using EDennis.AspNet.Base.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Middleware {
    public class MockUserMiddleware {

        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<MockUserOptions> _options;

        public MockUserMiddleware(RequestDelegate next,
            IOptionsMonitor<MockUserOptions> options) {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context) {

            if (context.Request.Path.Value.Contains("swagger")
                || !_options.CurrentValue.Enabled)
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
    }



}


