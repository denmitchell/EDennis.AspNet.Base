using Hr.BlazorApp.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hr.BlazorApp.Data.Services {
    public interface IStateService {
        Task<IEnumerable<State>> GetAllAsync();
    }
}
