using System.Net.Http;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public interface ITokenService {
        Task AssignTokenAsync(HttpClient client);
    }
}