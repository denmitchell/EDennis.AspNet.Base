using IdentityModel;
using System.Linq;
using System.Security.Claims;

namespace EDennis.NetStandard.Base {
    public static class ClaimsPrincipalExtensions {
        public static string Email(this ClaimsPrincipal cp) {
            return cp.Claims.Where(c => c.Type == JwtClaimTypes.Email || c.Type == ClaimTypes.Email).FirstOrDefault()?.Value;
        }
        public static string Phone(this ClaimsPrincipal cp) {
            return cp.Claims.Where(c => c.Type == JwtClaimTypes.PhoneNumber 
                || c.Type == ClaimTypes.HomePhone
                || c.Type == ClaimTypes.MobilePhone
                || c.Type == ClaimTypes.OtherPhone).FirstOrDefault()?.Value;
        }
        public static string Name(this ClaimsPrincipal cp) {
            return cp.Claims.Where(c => c.Type == JwtClaimTypes.Name || c.Type == ClaimTypes.Name).FirstOrDefault()?.Value;
        }
        public static string Subject(this ClaimsPrincipal cp) {
            return cp.Claims.Where(c => c.Type == JwtClaimTypes.Subject || c.Type == ClaimTypes.Sid).FirstOrDefault()?.Value;
        }
        public static string ClaimValue(this ClaimsPrincipal cp, string ClaimType) {
            return cp.Claims.Where(c => c.Type == ClaimType).FirstOrDefault()?.Value;
        }
        public static string[] ClaimValues(this ClaimsPrincipal cp, string ClaimType) {
            return cp.Claims.Where(c => c.Type == ClaimType).Select(c => c.Value).ToArray();
        }

    }
}
