using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.Middleware {

    public class AutoAuthenticationHandler: DelegatingHandler {

        public static Regex RequestVerificationTokenRegEx = new Regex("(?<=<input\\s+name\\s*=\\s*\"__RequestVerificationToken\"\\s+type\\s*=\\s*\"hidden\"\\s+value\\s*=\\s*\")[A-Za-z0-9_-]+");
        public static Regex AuthorizationCodeRegEx = new Regex("(?<=(\\?|&)code=)[A-Za-z0-9_-]+\\w+");


        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {            

            var response = await base.SendAsync(request, cancellationToken);
            
            var statusCode = (int)response.StatusCode;

            //get __RequestVerificationToken for login POST
            if (statusCode == 200 && request.RequestUri.AbsolutePath.Contains("/Account/Login?")) {
                var content = await response.Content.ReadAsStringAsync();
                return RequestVerificationTokenResponse(content);

                //get authorization code for request to token endpoint
            } else if (statusCode >= 300 && statusCode <= 399) {
                request.RequestUri = response.Headers.Location;
                var location = request.RequestUri.ToString();
                if (location.Contains("?code=") || location.Contains("&code="))
                    return AuthorizationCodeResponse(location);
                else
                    response = await base.SendAsync(request, cancellationToken);
            }

            return response;
        }


        protected HttpResponseMessage AuthorizationCodeResponse(string location) {
            var response = new HttpResponseMessage();
            var code = AuthorizationCodeRegEx.Match(location).Value;
            response.Content = new StringContent(code);
            return response;
        }

        protected HttpResponseMessage RequestVerificationTokenResponse(string body) {
            var response = new HttpResponseMessage();
            var code = RequestVerificationTokenRegEx.Match(body).Value;
            response.Content = new StringContent(code);
            return response;
        }



    }
}
