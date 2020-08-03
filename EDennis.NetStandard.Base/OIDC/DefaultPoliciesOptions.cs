namespace EDennis.NetStandard.Base {
    public class DefaultPoliciesOptions : OidcOptions {
        //NOTE: No longer needed.  The DomainRoleClaimsCache and
        //DomainRoleClaimsTransformer allows user roles to be
        //resolved to scope claims in the target application
        //public string UserScopePrefix { get; set; } = "user_";
        
        public string ExclusionPrefix { get; set; } = "-";
    }
}
