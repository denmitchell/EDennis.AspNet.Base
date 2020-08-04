using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Used in conjunction with ClientCredentialsTokenService as a replacement for
    /// AuthenticationBuilder.AddJwtBearer() extension method, which requires
    /// .Net Core 3.1.  (This library uses .Net Standard)
    /// </summary>
    /// <see cref="ClientCredentialsTokenService"/>
    /// <see cref="IServiceCollectionExtensions_Security"/>
    public class BearerTokenHandler : AuthenticationHandler<BearerTokenOptions> {


        private readonly ITokenService _tokenService;

        public BearerTokenHandler(ITokenService tokenService,
            IOptionsMonitor<BearerTokenOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock) {
            _tokenService = tokenService;
        }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (!Request.Headers.TryGetValue(BearerTokenOptions.HEADER_KEY, out StringValues authHeaderValue)) {
                Logger.LogInformation("No Authorization header");
                return AuthenticateResult.NoResult();
            }

            var authHeader = authHeaderValue.ToString();
            string token;

            if (!authHeader.StartsWith(BearerTokenOptions.HEADER_VALUE_PREFIX, StringComparison.OrdinalIgnoreCase)) {
                Logger.LogInformation("No 'Bearer ' in Authorization header");
                return AuthenticateResult.NoResult();
            } else {
                token = authHeader.Substring(BearerTokenOptions.HEADER_VALUE_PREFIX.Length).Trim();
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
