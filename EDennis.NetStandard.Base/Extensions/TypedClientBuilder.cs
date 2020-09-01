using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Net.Http;

namespace EDennis.NetStandard.Base {
    public class TypedClientBuilder {
        public IConfiguration Configuration { get; set; }
        public IServiceCollection Services { get; set; }
        public string ConfigKeyParent { get; set; }


        public TypedClientBuilder AddProxyClient(string clientName) => AddApiClient<object,object>(clientName);

        public TypedClientBuilder AddApiClient<TClientImplementation>(string clientName = null)
            where TClientImplementation : class => AddApiClient<TClientImplementation, TClientImplementation>(clientName);

        public TypedClientBuilder AddApiClient<TClientInterface, TClientImplementation>(string clientName = null)
            where TClientInterface : class
            where TClientImplementation : class, TClientInterface {

            clientName ??= typeof(TClientInterface).Name;

            var baseUrl = Configuration.GetValueOrThrow<string>($"{ConfigKeyParent}:{clientName}");

            Services.AddHttpClient(clientName, options =>
            {
                options.BaseAddress = new Uri(baseUrl);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
                UseCookies = false
            });


            if(typeof(TClientInterface) != typeof(object))
                Services.TryAddTransient<TClientInterface, TClientImplementation>();

            return this;
        }


    }
}
