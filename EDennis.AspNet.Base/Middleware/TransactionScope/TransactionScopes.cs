using System.Collections.Concurrent;
using System.Transactions;

namespace EDennis.AspNet.Base.Middleware {

    /// <summary>
    /// Singleton Lifetime
    /// </summary>
    public class TransactionScopes : ConcurrentDictionary<string,TransactionScope>{
    }
}
