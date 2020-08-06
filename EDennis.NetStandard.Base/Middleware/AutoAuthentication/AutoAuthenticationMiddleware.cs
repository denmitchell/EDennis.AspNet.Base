using EDennis.NetStandard.Base;
using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    //note: requires services.AddHttpClient("AutoAuthenticationClient",c=>{...})
    //                   .AddHttpMessageHandler<AutoAuthenticationHandler>();

    /// <summary>
    /// This middleware generates and adds to the request an Authorization header 
    /// with a Bearer token representing the access token result of an OAuth/OIDC
    /// workflow.
    /// This middleware supports OAuth client_credentials flow (machine to machine)
    /// and OIDC authorization_code flow with PKCE (interactive).
    /// </summary>
    public class AutoAuthenticationMiddleware {

        private readonly RequestDelegate _next;
        private readonly AutoAuthenticationOptions _options;
        private readonly AuthorizationCodeOptions _oidcOptions;
        private readonly IHttpClientFactory _factory;


        public AutoAuthenticationMiddleware(RequestDelegate next, 
            IOptionsMonitor<AutoAuthenticationOptions> options,
            IOptionsMonitor<AuthorizationCodeOptions> oidcOptions,
            IHttpClientFactory factory) {
            _next = next;
            _factory = factory;
            _options = options.CurrentValue;
            _oidcOptions = oidcOptions.CurrentValue;
            if (!_oidcOptions.Authority.EndsWith("/"))
                _oidcOptions.Authority += "/";
        }


        public async Task InvokeAsync(HttpContext context) {

            if (!_options.Enabled)
                await _next(context);
            else {
                var client = _factory.CreateClient("MockOidcClient");
                client.BaseAddress = new Uri(_oidcOptions.Authority);

                var tokenResponse = await LoginAsync(client);

                context.Request.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
 
                await _next(context); 
            }

        }



        public async Task<OidcTokenResponse> LoginAsync(HttpClient client) {

            var disco = await client.GetDiscoveryDocumentAsync(client.BaseAddress.ToString());
            if (disco.IsError) {
                throw new ApplicationException($"Cannot retrieve Discovery Document at {client.BaseAddress}"); ;
            }

            if (_oidcOptions.GrantType == "client_credentials") {
                var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest {
                    Address = disco.TokenEndpoint,

                    ClientId = _oidcOptions.ClientId,
                    ClientSecret = _oidcOptions.ClientSecret,
                    Scope = string.Join(" ", _oidcOptions.Scopes)
                });
                if (tokenResponse.IsError) {
                    throw new ApplicationException($"Client Credentials flowed failed: {tokenResponse.Error}: {tokenResponse.ErrorDescription}"); ;
                }
                return new OidcTokenResponse {
                    AccessToken = tokenResponse.AccessToken,
                    RefreshToken = tokenResponse.RefreshToken,
                    Scope = tokenResponse.Scope,
                    ExpiresIn = tokenResponse.ExpiresIn
                };
            } else if (_oidcOptions.GrantType == "authorization_code") {

                PrepareAuthorizeRequest(disco, out string codeVerifier, out string authorizeUrl);
                var response = await client.GetAsync(authorizeUrl);
                var requestVerificationToken = await response.Content.ReadAsStringAsync();

                var msg = GetLoginRequestMessage(requestVerificationToken, authorizeUrl);
                response = await client.SendAsync(msg);
                var authorizationCode = await response.Content.ReadAsStringAsync();

                msg = GetTokenRequestMessage(disco, authorizationCode, codeVerifier);
                response = await client.SendAsync(msg);
                var responseBody = await response.Content.ReadAsStringAsync();
                var oidcTokenResponse = JsonSerializer.Deserialize<OidcTokenResponse>(responseBody);

                return oidcTokenResponse;

            } else {
                throw new ApplicationException($"LoginAsync failed.  No support for grant type = {_oidcOptions.GrantType}");
            }


        }

        public async Task LogoutAsync(HttpClient client, string idToken) {
            //GET /connect/endsession?id_token_hint=eyJhbGciOiJSUzI1NiIsImtpZCI6IjdlOGFkZmMzMjU1OTEyNzI0ZDY4NWZmYmIwOThjNDEyIiwidHlwIjoiSldUIn0.eyJuYmYiOjE0OTE3NjUzMjEsImV4cCI6MTQ5MTc2NTYyMSwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo1MDAwIiwiYXVkIjoianNfb2lkYyIsIm5vbmNlIjoiYTQwNGFjN2NjYWEwNGFmNzkzNmJjYTkyNTJkYTRhODUiLCJpYXQiOjE0OTE3NjUzMjEsInNpZCI6IjI2YTYzNWVmOTQ2ZjRiZGU3ZWUzMzQ2ZjFmMWY1NTZjIiwic3ViIjoiODg0MjExMTMiLCJhdXRoX3RpbWUiOjE0OTE3NjUzMTksImlkcCI6ImxvY2FsIiwiYW1yIjpbInB3ZCJdfQ.STzOWoeVYMtZdRAeRT95cMYEmClixWkmGwVH2Yyiks9BETotbSZiSfgE5kRh72kghN78N3-RgCTUmM2edB3bZx4H5ut3wWsBnZtQ2JLfhTwJAjaLE9Ykt68ovNJySbm8hjZhHzPWKh55jzshivQvTX0GdtlbcDoEA1oNONxHkpDIcr3pRoGi6YveEAFsGOeSQwzT76aId-rAALhFPkyKnVc-uB8IHtGNSyRWLFhwVqAdS3fRNO7iIs5hYRxeFSU7a5ZuUqZ6RRi-bcDhI-djKO5uAwiyhfpbpYcaY_TxXWoCmq8N8uAw9zqFsQUwcXymfOAi2UF3eFZt02hBu-shKA&post_logout_redirect_uri=http%3A%2F%2Flocalhost%3A7017%2Findex.html
            var logoutUrl = GetLogoutUrl(idToken);
            await client.GetAsync(logoutUrl);
        }


        #region Helper Methods

        private string GetLogoutUrl(string idToken) {
            var url = $"{_oidcOptions.Authority }connect/endsession";
            url += $"?id_token_hint={idToken}";
            return url;
        }


        private HttpRequestMessage GetTokenRequestMessage(DiscoveryDocumentResponse disco,
            string authorizationCode, string codeVerifier) {

            var tokenUrl = disco.TokenEndpoint;

            var parameters = new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string>("grant_type","authorization_code"),
                        new KeyValuePair<string, string>("code",authorizationCode),
                        new KeyValuePair<string, string>("code_verifier",codeVerifier),
                        new KeyValuePair<string, string>("client_id",_oidcOptions.ClientId),
                        new KeyValuePair<string, string>("redirect_uri",_oidcOptions.RedirectUri)
                    };
            if (_oidcOptions.ClientSecret != null)
                parameters.Add(new KeyValuePair<string, string>("client_secret", _oidcOptions.ClientSecret));

            var msg = new HttpRequestMessage() {
                Content = new FormUrlEncodedContent(parameters),
                Method = HttpMethod.Post,
                RequestUri = new Uri(tokenUrl)
            };

            return msg;

        }


        private HttpRequestMessage GetLoginRequestMessage(string requestVerificationToken,
            string authorizeUrl) {
            var startIndex = $"{_oidcOptions.Authority }connect/authorize?".Length;

            var encoded = System.Net.WebUtility.UrlEncode("/connect/authorize/callback?"
                        + authorizeUrl.Substring(startIndex));
            var loginUrl = $"{_oidcOptions.Authority}Account/Login?ReturnUrl={encoded}";

            var msg = new HttpRequestMessage() {
                Content = new FormUrlEncodedContent(
                    new KeyValuePair<string, string>[] {
                        new KeyValuePair<string, string>("Input.Email",_options.AutoLoginUsername),
                        new KeyValuePair<string, string>("Input.Password",_options.AutoLoginPassword),
                        new KeyValuePair<string, string>("__RequestVerificationToken",requestVerificationToken),
                        new KeyValuePair<string, string>("Input.RememberMe","false")
                    }
                ),
                Method = HttpMethod.Post,
                RequestUri = new Uri(loginUrl)
            };

            return msg;
        }


        private void PrepareAuthorizeRequest(DiscoveryDocumentResponse disco,
            out string codeVerifier, out string authorizeUrl) {

            var cvBytes = GetCodeVerifier();
            codeVerifier = Encoding.UTF8.GetString(cvBytes);
            var codeChallenge = GetCodeChallenge(cvBytes);
            var nonce = GetNonce();
            var state = GetState();

            var ru = new RequestUrl(disco.AuthorizeEndpoint);
            authorizeUrl = ru.CreateAuthorizeUrl(
                clientId: _oidcOptions.ClientId,
                responseType: _oidcOptions.ResponseType,
                redirectUri: $"{ _oidcOptions.RedirectUri}",
                nonce: nonce,
                state: state,
                codeChallenge: codeChallenge,
                codeChallengeMethod: _oidcOptions.CodeChallengeMethod,
                scope: string.Join(" ", _oidcOptions.Scopes)
                );

        }




        private static string GetNonce() {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[24];
            rng.GetBytes(bytes);
            var base64 = Base64UrlEncode(bytes);
            return Encoding.UTF8.GetString(base64, 0, base64.Length); ;
        }


        private static string GetState() {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[24];
            rng.GetBytes(bytes);
            var base64 = Base64UrlEncode(bytes);
            return Encoding.UTF8.GetString(base64, 0, base64.Length); ;
        }


        private static byte[] GetCodeVerifier() {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[33];
            rng.GetBytes(bytes);
            var base64 = Base64UrlEncode(bytes);
            return base64;
        }


        private static string GetCodeChallenge(byte[] bytes) {
            var sha = SHA256.Create();
            var challenge = Base64UrlEncode(sha.ComputeHash(bytes));
            return Encoding.UTF8.GetString(challenge, 0, challenge.Length);
        }


        private static byte[] Base64UrlEncode(byte[] b) {

            var len = (int)Math.Ceiling(b.Length * 4.0 / 3);
            var chunkCount = (int)Math.Floor(len / 4.0);
            var remainder = b.Length % 3;

            byte[] b64 = new byte[len];
            int k = 0;
            for (int i = 0; i < chunkCount; i++) {
                //var x = b[i*3] >> 2;
                //Console.WriteLine($"{k}: {Convert.ToString(x,2).PadLeft(32,'0')}");
                b64[k] = (byte)charMap[(byte)(b[i * 3] >> 2)];

                //x = ( ( b[i*3] & 0b_0000_0011 ) << 6 >> 2 ) | ( b[i*3+1] >> 4 );
                //Console.WriteLine($"{k+1}: {Convert.ToString(x,2).PadLeft(32,'0')}");
                b64[k + 1] = (byte)charMap[(byte)(((b[i * 3] & 0b_0000_0011) << 6 >> 2) | (b[i * 3 + 1] >> 4))];

                //x = ( ( b[i*3+1] & 0b_0000_1111 ) << 4 >> 2 ) | ( b[i*3+2] >> 6 );
                //Console.WriteLine($"{k+2}: {Convert.ToString(x,2).PadLeft(32,'0')}");
                b64[k + 2] = (byte)charMap[(byte)(((b[i * 3 + 1] & 0b_0000_1111) << 4 >> 2) | (b[i * 3 + 2] >> 6))];

                //x = ( b[i*3+2] & 0b_0011_1111 ) << 2 >> 2;
                //Console.WriteLine($"{k+3}: {Convert.ToString(x,2).PadLeft(32,'0')}");			
                b64[k + 3] = (byte)charMap[(byte)((b[i * 3 + 2] & 0b_0011_1111) << 2 >> 2)];

                k += 4;
            }
            if (remainder > 0) {
                b64[k] = (byte)charMap[(byte)(b[30] >> 2)];
                b64[k + 1] = (byte)((b[30] & 0b_0000_0011) << 6 >> 2);
                if (remainder == 1)
                    b64[k + 1] = (byte)charMap[b64[k + 1]];
                else {
                    b64[k + 1] = (byte)charMap[(byte)(b64[k + 1] | (b[31] >> 4))];
                    b64[k + 2] = (byte)charMap[(byte)((b[31] & 0b_0000_1111) << 4 >> 2)];
                }
            }
            return b64;
        }


        private readonly static Dictionary<byte, char> charMap = new Dictionary<byte, char> {
        { 0b_0000_0000, 'A' }, { 0b_0000_0001, 'B' }, { 0b_0000_0010, 'C' }, { 0b_0000_0011, 'D' },
        { 0b_0000_0100, 'E' }, { 0b_0000_0101, 'F' }, { 0b_0000_0110, 'G' }, { 0b_0000_0111, 'H' },
        { 0b_0000_1000, 'I' }, { 0b_0000_1001, 'J' }, { 0b_0000_1010, 'K' }, { 0b_0000_1011, 'L' },
        { 0b_0000_1100, 'M' }, { 0b_0000_1101, 'N' }, { 0b_0000_1110, 'O' }, { 0b_0000_1111, 'P' },
        { 0b_0001_0000, 'Q' }, { 0b_0001_0001, 'R' }, { 0b_0001_0010, 'S' }, { 0b_0001_0011, 'T' },
        { 0b_0001_0100, 'U' }, { 0b_0001_0101, 'V' }, { 0b_0001_0110, 'W' }, { 0b_0001_0111, 'X' },
        { 0b_0001_1000, 'Y' }, { 0b_0001_1001, 'Z' }, { 0b_0001_1010, 'a' }, { 0b_0001_1011, 'b' },
        { 0b_0001_1100, 'c' }, { 0b_0001_1101, 'd' }, { 0b_0001_1110, 'e' }, { 0b_0001_1111, 'f' },
        { 0b_0010_0000, 'g' }, { 0b_0010_0001, 'h' }, { 0b_0010_0010, 'i' }, { 0b_0010_0011, 'j' },
        { 0b_0010_0100, 'k' }, { 0b_0010_0101, 'l' }, { 0b_0010_0110, 'm' }, { 0b_0010_0111, 'n' },
        { 0b_0010_1000, 'o' }, { 0b_0010_1001, 'p' }, { 0b_0010_1010, 'q' }, { 0b_0010_1011, 'r' },
        { 0b_0010_1100, 's' }, { 0b_0010_1101, 't' }, { 0b_0010_1110, 'u' }, { 0b_0010_1111, 'v' },
        { 0b_0011_0000, 'w' }, { 0b_0011_0001, 'x' }, { 0b_0011_0010, 'y' }, { 0b_0011_0011, 'z' },
        { 0b_0011_0100, '0' }, { 0b_0011_0101, '1' }, { 0b_0011_0110, '2' }, { 0b_0011_0111, '3' },
        { 0b_0011_1000, '4' }, { 0b_0011_1001, '5' }, { 0b_0011_1010, '6' }, { 0b_0011_1011, '7' },
        { 0b_0011_1100, '8' }, { 0b_0011_1101, '9' }, { 0b_0011_1110, '-' }, { 0b_0011_1111, '_' } };

        #endregion


    }

    public static class IServiceCollectionExtensions_AutoAuthenticationMiddleware {
        public static IServiceCollection AddAutoAuthentication(this IServiceCollection services, IConfiguration config) {
            services.Configure<AutoAuthenticationOptions>(config.GetSection("Security:AutoAuthentication"));
            return services;
        }
    }

    public static class IApplicationBuilderExtensions_AutoAuthenticationMiddleware {
        public static IApplicationBuilder UseAutoAuthentication(this IApplicationBuilder app) {
            app.UseMiddleware<AutoAuthenticationMiddleware>();
            return app;
        }

        public static IApplicationBuilder UseAutoAuthenticationFor(this IApplicationBuilder app,
            params string[] startsWithSegments) {
            app.UseWhen(context =>
            {
                foreach (var partialPath in startsWithSegments)
                    if (context.Request.Path.StartsWithSegments(partialPath))
                        return true;
                return false;
            },
                app => app.UseAutoAuthentication()
            );
            return app;
        }

        public static IApplicationBuilder UseAutoAuthenticationWhen(this IApplicationBuilder app,
            Func<HttpContext, bool> predicate) {
            app.UseWhen(predicate, app => app.UseAutoAuthentication());
            return app;
        }


    }


}
