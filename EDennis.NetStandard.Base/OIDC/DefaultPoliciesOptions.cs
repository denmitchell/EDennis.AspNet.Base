namespace EDennis.NetStandard.Base {
    public class DefaultPoliciesOptions : OidcOptions {
        public string UserScopePrefix { get; set; } = "user_";
        public string ExclusionPrefix { get; set; } = "-";
    }
}
