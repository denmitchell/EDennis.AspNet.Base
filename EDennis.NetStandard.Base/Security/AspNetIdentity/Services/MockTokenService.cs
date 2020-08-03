using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class MockTokenService : ITokenService {

        private readonly ILogger<MockTokenService> _logger;

        public MockTokenService (ILogger<MockTokenService> logger) {
            _logger = logger;
        }

        public async Task AssignTokenAsync(HttpClient client) {
            await Task.Run(() => { 
                _logger.LogDebug($"MockTokenService.AssignTokenAsync called for HttpClient @{client.BaseAddress}"); 
            });
        }
    }
}
