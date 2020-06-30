using Hr.App.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hr.App.Data.Services {
    public interface IStateService {
        Task<IEnumerable<State>> GetAllAsync();
    }
}
