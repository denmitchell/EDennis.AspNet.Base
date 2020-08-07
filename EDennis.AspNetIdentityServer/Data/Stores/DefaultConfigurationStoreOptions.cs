using IdentityServer4.EntityFramework.Options;

namespace EDennis.AspNetIdentityServer {
    public class DefaultConfigurationStoreOptions : ConfigurationStoreOptions {

        public void Load(ConfigurationStoreOptions options) {
            var props = options.GetType().GetProperties();
            foreach (var prop in props) {
                prop.SetValue(options, prop.GetValue(this));                
            }
        }

    }
}
