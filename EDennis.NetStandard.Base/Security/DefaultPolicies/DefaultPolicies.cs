using System.Collections.Generic;

namespace EDennis.NetStandard.Base
{
    public class DefaultPolicies : Dictionary<string,List<string>>{
        public const string DEFAULT_CLAIMTYPES_KEY = "Security:DefaultPolicies:ClaimTypes";
        public const string DEFAULT_POLICIES_KEY = "Security:DefaultPolicies:Policies";
    }
}
