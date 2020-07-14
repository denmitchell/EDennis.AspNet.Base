using EDennis.AspNet.Base.Middleware.MockUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
                var claims = _options.CurrentValue.Claims.SelectMany(c=>new Claim(c.Key, c.Value))
                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        _options.CurrentValue.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)),
                        "mockAuth"
                        ));
                 
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

/*
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
					
public class Program
{
	public static void Main()
	{
		var dict = new Dictionary<string,List<string>>();
		dict.Add("Red",new List<string>{"Maroon","Burgundy","Pink","Red"});
		dict.Add("Blue",new List<string>{"Navy","Indigo","Periwinkle","Blue"});
		
		var flattened = dict.Aggregate(new List<KeyValuePair<string,string>>(), 
											  (list, entry) => list.Union(entry.Value.Aggregate(new List<KeyValuePair<string,string>>(), 
																(values,value) => values.Union(new List<KeyValuePair<string,string>>() { KeyValuePair.Create(entry.Key,value) }).ToList())).ToList()
									  );
		
		var flattened2 = dict.Flatten<string,List<string>,string>();
		Console.WriteLine(JsonSerializer.Serialize(flattened2,new JsonSerializerOptions{WriteIndented=true}));
	}
}

public static class DictionaryExtensionMethods {
	public static IEnumerable<KeyValuePair<K,V>> Flatten<K,E,V> (this Dictionary<K,E> dict)
	where E : IEnumerable<V> {
		return dict.Aggregate(new List<KeyValuePair<K,V>>(), 
											  (list, entry) => list.Union(entry.Value.Aggregate(new List<KeyValuePair<K,V>>(), 
																(values,value) => values.Union(new List<KeyValuePair<K,V>>() { KeyValuePair.Create(entry.Key,value) }).ToList())).ToList());
				
	}
}
*/
