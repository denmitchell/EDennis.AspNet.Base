namespace EDennis.NetStandard.Base {
    public class DbContextProvider<TContext> {
        public TContext DbContext { get; set; }

        public DbContextProvider(TContext context) {
            DbContext = context;
        }
    }
}
