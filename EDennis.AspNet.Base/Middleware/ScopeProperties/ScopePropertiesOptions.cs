
namespace EDennis.AspNet.Base.Middleware {
    public class ScopePropertiesOptions {
        public string[] ClaimTypesToInclude { get; set; } = new string[] { };
        public string[] ClaimTypesToPropagate { get; set; } = new string[] { };
    }
}
