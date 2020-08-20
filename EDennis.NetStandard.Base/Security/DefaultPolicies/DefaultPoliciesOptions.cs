using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EDennis.NetStandard.Base
{
    public class DefaultPoliciesOptions{
        public const string DEFAULT_CLAIMTYPES_KEY = "Security:DefaultPolicies:ClaimTypes";
        public const string DEFAULT_POLICIES_KEY = "Security:DefaultPolicies:Policies";
    }
}
