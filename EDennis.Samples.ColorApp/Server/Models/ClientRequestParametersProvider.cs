using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.Samples.ColorApp.Server.Models {
    public class ClientRequestParametersProvider : IClientRequestParametersProvider {

        private readonly string _authority;
        private readonly string _clientId;
        private readonly string _redirectUri;
        private readonly string _postLogoutRedirectUri;
        private readonly string _responseType;
        private readonly string _scope;


        public ClientRequestParametersProvider(string authority, string clientId, string redirectUri, 
            string postLogoutRedirectUri, string responseType, string scope) {
            _authority = authority;
            _clientId = clientId;
            _redirectUri = redirectUri;
            _postLogoutRedirectUri = postLogoutRedirectUri;
            _responseType = responseType;
            _scope = scope;
        }

        public IDictionary<string, string> GetClientParameters(HttpContext context, string clientId) {
            return new Dictionary<string, string> {
                ["authority"] = _authority,
                ["client_id"] = _clientId,
                ["redirect_uri"] = _redirectUri,
                ["post_logout_redirect_uri"] = _postLogoutRedirectUri,
                ["response_type"] = _responseType,
                ["scope"] = _scope
            };

        }
    }
}
