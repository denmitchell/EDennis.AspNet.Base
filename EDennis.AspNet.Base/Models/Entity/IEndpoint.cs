using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base {
    public interface IEndpoint {
        string GetIdEndpoint();
        string GetSysIdEndpoint();
    }
}
