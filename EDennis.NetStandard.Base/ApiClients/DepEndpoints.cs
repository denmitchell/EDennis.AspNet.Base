using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace EDennis.NetStandard.Base {

    public class TypedClientBuilder {
        public DepEndpoints DepEndpoints { get; set; }
        public IServiceCollection Services { get; set; }

        public TypedClientBuilder AddClient<TClientInterface, TClientImplementation>()
        where TClientInterface : class
        where TClientImplementation : class, TClientInterface {


            var (ClientName, BaseAddress) = DepEndpoints.Lookup<TClientImplementation>();
            
            Services.AddHttpClient(ClientName, options =>
            {
                options.BaseAddress = BaseAddress;
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler {
                UseCookies = false
            });

            Services.AddTransient<TClientInterface, TClientImplementation>();

            return this;
        }


    }




    public class DepEndpoints : Dictionary<string, Dictionary<string, Dictionary<string, string[]>>> {

        public (string ClientName, Uri BaseAddress) Lookup<TClientImplementation>() {

            var clientName = typeof(TClientImplementation).Name;

            if (clientName.EndsWith("Controller"))
                clientName = clientName.Substring(0, clientName.Length - "Controller".Length);

            string baseAddressString = null;
            foreach (var scheme in Keys)
                foreach (var host in this[scheme].Keys)
                    foreach (var port in this[scheme][host].Keys)
                        if (this[scheme][host][port].Any(x => x == clientName))
                            baseAddressString = $"{scheme}:{host}:{port}";
            if (baseAddressString == null)
                throw new ArgumentException($"Client {clientName} not found in DepEndPoints section of configuration");

            Uri baseAddress = null;
            try {
                baseAddress = new Uri(baseAddressString);
            } catch (Exception) {
                throw new ArgumentException($"Cannot convert {baseAddressString} to URI");
            }

            return (clientName, baseAddress);
        }

    }
}
