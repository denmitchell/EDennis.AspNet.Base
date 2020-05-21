using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace EDennis.AspNet.Base.Middleware {
    public class TransactionScopes : ConcurrentDictionary<string,TransactionScope>{
    }
}
