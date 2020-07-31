﻿namespace EDennis.NetStandard.Base.Security {
    public class DefaultPoliciesOptions : OidcOptions {
        public string UserScopePrefix { get; set; } = "user_";
        public string ExclusionPrefix { get; set; } = "-";
    }
}