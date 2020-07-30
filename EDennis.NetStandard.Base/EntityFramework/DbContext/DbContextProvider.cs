using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base.EntityFramework {
    public class DbContextProvider<TContext> {
        public TContext DbContext { get; set; }

        public DbContextProvider(TContext context) {
            DbContext = context;
        }
    }
}
