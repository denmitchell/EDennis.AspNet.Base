using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace EDennis.AspNet.Base.Middleware {

    /// <summary>
    /// Singleton
    /// </summary>
    public class Jwt {

        public SigningCredentials SigningCredentials { get; set; }
        public JwtSecurityToken JwtSecurityToken { get; set; }

        private readonly JwtOptions _jwtOptions;
        private readonly JwtParams _jwtParams;
        private JsonWebKey _jwk;

        public Jwt(IOptionsMonitor<JwtOptions> jwtOptions, IOptionsMonitor<JwtParamSets> jwtParamSets) {
            _jwtOptions = jwtOptions.CurrentValue;
            _jwtParams = jwtParamSets.CurrentValue.FirstOrDefault(p => p.kid == _jwtOptions.Kid);
            BuildSigningCredentials();
        }

        private void BuildSigningCredentials() {
            if (_jwtParams.kty == JwtKty.RSA) {
                _jwk = new JsonWebKey {
                    Kid = _jwtParams.kid,
                    N = _jwtParams.n,
                    E = _jwtParams.e,
                    D = _jwtParams.d,
                    P = _jwtParams.p,
                    Q = _jwtParams.q,
                    DP = _jwtParams.dp,
                    DQ = _jwtParams.dq,
                    QI = _jwtParams.qi,
                    Kty = _jwtParams.kty.ToString()                    
                };                
                SigningCredentials = new SigningCredentials(_jwk,
                    SecurityAlgorithms.RsaSha256Signature);
            } else if (_jwtParams.kty == JwtKty.oct) {
                _jwk = new JsonWebKey {
                    Kid = _jwtParams.kid,
                    K = _jwtParams.k, 
                    Kty = _jwtParams.kty.ToString() 
                };
                SigningCredentials = new SigningCredentials(_jwk,
                    SecurityAlgorithms.HmacSha256Signature);
            }
        }

        public string GenerateTokenAsync(string audience, Claim[] claims) {
            JwtSecurityToken = new JwtSecurityToken(
                issuer: "https://localhost:5000", 
                notBefore: DateTime.Now, 
                expires: DateTime.Now.AddHours(_jwtOptions.ExpirationHours),
                signingCredentials: SigningCredentials,
                audience: audience
                );
            JwtSecurityToken.Payload.AddClaims(claims);

            var tokenHandler = new JwtSecurityTokenHandler();


            var token =  tokenHandler.WriteToken(JwtSecurityToken);

            //typically, this would occur in the API or APP
            var cp = tokenHandler.ValidateToken(token, 
                new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _jwk,
                }, out SecurityToken securityToken);
            return token;
        }

        private static byte[] CopyAndReverse(byte[] data) {
            byte[] reversed = new byte[data.Length];
            Array.Copy(data, 0, reversed, 0, data.Length);
            Array.Reverse(reversed);
            return reversed;
        }


        /*
         var rsa = RSA.Create(new RSAParameters {
                    Modulus = WebEncoders.Base64UrlDecode(_jwtParams.n),
                    Exponent = WebEncoders.Base64UrlDecode(_jwtParams.e),
                    D = WebEncoders.Base64UrlDecode(_jwtParams.d),
                    P = WebEncoders.Base64UrlDecode(_jwtParams.p),
                    Q = WebEncoders.Base64UrlDecode(_jwtParams.q),
                    DP = WebEncoders.Base64UrlDecode(_jwtParams.dp),
                    DQ = WebEncoders.Base64UrlDecode(_jwtParams.dq),
                    InverseQ = WebEncoders.Base64UrlDecode(_jwtParams.qi)
                });


          
         */

    }
}
