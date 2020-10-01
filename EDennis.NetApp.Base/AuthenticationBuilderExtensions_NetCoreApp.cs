using EDennis.NetStandard.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EDennis.NetApp.Base {
    public static class AuthenticationBuilderExtensions_NetCoreApp {

        public static AuthenticationBuilder AddOpenIdConnect(this AuthenticationBuilder builder, IConfiguration config,
            string configKey = "Security:OpenIdConnect") {

            var settings = new OpenIdConnectSettings();
            config.BindSectionOrThrow(configKey, settings);

            builder
                .AddOpenIdConnect("oidc",
                    OpenIdConnectDefaults.DisplayName,
                    opt => settings.LoadOptions(opt)
            );

            return builder;
        }


    }

}
