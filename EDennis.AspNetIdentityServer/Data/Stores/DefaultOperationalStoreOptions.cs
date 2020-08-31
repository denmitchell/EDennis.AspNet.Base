using IdentityServer4.EntityFramework.Options;

namespace EDennis.AspNetIdentityServer {
    public class DefaultOperationalStoreOptions : OperationalStoreOptions {

        public void Load(OperationalStoreOptions options) {
            var props = options.GetType().GetProperties();
            foreach (var prop in props) {
                prop.SetValue(options, prop.GetValue(this));                
            }
        }

    }
}
