using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public interface ITokenService {
        Task AssignTokenAsync(HttpClient client);
        Task<ClaimsPrincipal> ValidateTokenAsync(string token);
    }
}