using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.Middleware.TokenAuthentication {
    public class TokenAuthenticationMiddleware : AuthenticationMiddleware {
        public TokenAuthenticationMiddleware(RequestDelegate next, IAuthenticationSchemeProvider schemes) : base(next, schemes) {
            Schemes.AddScheme(new AuthenticationScheme(BearerTokenOptions.AUTHENTICATION_SCHEME,
                BearerTokenOptions.AUTHENTICATION_SCHEME, typeof(BearerTokenHandler)));
        }        
    }

    public class BearerTokenHandler : AuthenticationHandler<BearerTokenOptions> {

        
        private readonly ClientCredentialsTokenService _tokenService;

        public BearerTokenHandler(ClientCredentialsTokenService tokenService,
            IOptionsMonitor<BearerTokenOptions> options, ILoggerFactory logger, 
            UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock) {            
            _tokenService = tokenService;
        }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            string authorization = Request.Headers[BearerTokenOptions.HEADER_KEY];
            string token = "";

            // If no authorization header found, nothing to process further
            if (string.IsNullOrEmpty(authorization)) {
                return AuthenticateResult.NoResult();
            }

            if (authorization.StartsWith(BearerTokenOptions.HEADER_VALUE_PREFIX, StringComparison.OrdinalIgnoreCase)) {
                token = authorization.Substring(BearerTokenOptions.HEADER_VALUE_PREFIX.Length).Trim();
            }

            // If no token found, no further work possible
            if (string.IsNullOrEmpty(token)) {
                return AuthenticateResult.NoResult();
            }

            var cp = await _tokenService.ValidateTokenAsync(token);

            if (cp != null) {
                var ticket = new AuthenticationTicket(cp, new AuthenticationProperties(), 
                    BearerTokenOptions.AUTHENTICATION_SCHEME);

                ticket.Properties.StoreTokens(new[] {
                    new AuthenticationToken { Name = BearerTokenOptions.TOKEN_NAME, Value = token }
                });
                return AuthenticateResult.Success(ticket);
            } else {
                return AuthenticateResult.NoResult();
            }

        }
    }



    public class BearerTokenOptions : AuthenticationSchemeOptions {
        public const string AUTHENTICATION_SCHEME = "Bearer";
        public const string HEADER_KEY = "Authorization";
        public const string HEADER_VALUE_PREFIX = "Bearer ";
        public const string TOKEN_NAME = "access_token";
    }

}
