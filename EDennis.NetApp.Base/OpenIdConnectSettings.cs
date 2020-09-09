using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace EDennis.NetApp.Base {
    public class OpenIdConnectSettings {

        private static OpenIdConnectOptions _defaults = new OpenIdConnectOptions();

        public void LoadOptions(OpenIdConnectOptions options) {

            options.Authority = Authority;
            options.ClientId = ClientId;
            options.ClientSecret = ClientSecret;
            options.GetClaimsFromUserInfoEndpoint = GetClaimsFromUserInfoEndpoint;
            options.RequireHttpsMetadata = RequireHttpsMetadata;
            options.SaveTokens = SaveTokens;
            foreach(var item in Scope) 
                options.Scope.Add(item);
            options.ResponseMode = ResponseMode;
            options.ResponseType = ResponseType;
            options.UsePkce = UsePkce;

            options.SignInScheme = SignInScheme;
            options.SignOutScheme = SignOutScheme;
            options.AuthenticationMethod = AuthenticationMethod;
            options.RefreshOnIssuerKeyNotFound = RefreshOnIssuerKeyNotFound;
            options.Resource = Resource;
            options.Prompt = Prompt;

            options.MetadataAddress = MetadataAddress;
            options.CallbackPath = new PathString(CallbackPath);
            options.ReturnUrlParameter = ReturnUrlParameter;
            options.SignedOutCallbackPath = new PathString(SignedOutCallbackPath);
            options.SignedOutRedirectUri = SignedOutRedirectUri;
            options.RemoteSignOutPath = new PathString(RemoteSignOutPath);
            options.AccessDeniedPath = new PathString(AccessDeniedPath);

            options.TokenValidationParameters = new TokenValidationParameters {
                NameClaimType = JwtClaimTypes.Name,
                RoleClaimType = JwtClaimTypes.Role
            };

        }


        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool GetClaimsFromUserInfoEndpoint { get; set; }
        public bool RequireHttpsMetadata { get; set; } = true;
        public bool SaveTokens { get; set; } = true;
        public List<string> Scope { get; set; } = new List<string>();
        public string ResponseMode { get; set; } = OpenIdConnectResponseMode.FormPost;
        public string ResponseType { get; set; } = OpenIdConnectResponseType.Code;
        public bool UsePkce { get; set; } = true;


        public string SignInScheme { get; set; } = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        public string SignOutScheme { get; set; } = IdentityServerConstants.SignoutScheme;
        public OpenIdConnectRedirectBehavior AuthenticationMethod { get; set; } = OpenIdConnectRedirectBehavior.RedirectGet;
        public bool RefreshOnIssuerKeyNotFound { get; set; } = true;
        public string Resource { get; set; }
        public string Prompt { get; set; }


        public string MetadataAddress { get; set; }
        public string CallbackPath { get; set; } = _defaults.CallbackPath;
        public string ReturnUrlParameter { get; set; } = "ReturnUrl";
        public string SignedOutCallbackPath { get; set; } = _defaults.SignedOutCallbackPath;
        public string SignedOutRedirectUri { get; set; } = "/";
        public string RemoteSignOutPath { get; set; } = _defaults.RemoteSignOutPath;
        public string AccessDeniedPath { get; set; }

    }
}
