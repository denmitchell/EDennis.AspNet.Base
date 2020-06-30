using Hr.App.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hr.App.Data.Services {
    public interface IAddressService {
        Task<Address> GetAsync(int id);
        Task<IEnumerable<Address>> GetForPersonAsync(int personId);
        Task<Address> CreateAsync(Address address);
        Task<Address> UpdateAsync(Address address);
        Task DeleteAsync(int id);
    }
}
