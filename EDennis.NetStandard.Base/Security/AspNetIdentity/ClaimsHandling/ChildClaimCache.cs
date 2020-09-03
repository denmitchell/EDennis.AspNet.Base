using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Singleton cache used by IClaimsTransformation.
    /// This cache is an application/project-specific cache that is used to
    /// hold claims that are specific to the application.  This cache can be
    /// helpful in reducing size of identity or access tokens and/or for 
    /// implementing role-like functionality without using ASP.NET Identity 
    /// Roles. If desired, many highly granular child claims can be 
    /// maintained in the cache without much impact on performance.
    /// 
    /// The child claims are populated from data in configuration.  The Data
    /// section is a two-dimensional array where the first dimension is the
    /// row/object and the second dimension is the column/property.  For
    /// convenience, there are three configuration strategies:
    /// 
    /// <list type="bullet">
    /// <item>Define no constant values. In this strategy, each row
    /// consists of four columns:
    ///     <list type="number">
    ///         <item>ParentType</item>
    ///         <item>ParentValue</item>
    ///         <item>ChildType</item>
    ///         <item>ChildValue</item>
    ///     </list>
    /// </item>
    /// <item>Define ParentType as a constant value. For example, ParentType
    /// could be set as "role" or "role:{AppName}" (the latter being 
    /// consistent with DomainIdentity classes). In this strategy, each row
    /// consists of three columns:
    ///     <list type="number">
    ///         <item>ParentValue</item>
    ///         <item>ChildType</item>
    ///         <item>ChildValue</item>
    ///     </list>
    /// </item>
    /// <item>Define ParentType and ChildType as a constant values. For example, 
    /// ParentType could be set as "role" or "role:{AppName}" (the latter being 
    /// consistent with DomainIdentity classes), and ChildType could be set
    /// as "scope". In this strategy, each row consists of two columns:
    ///     <list type="number">
    ///         <item>ParentValue</item>
    ///         <item>ChildValue</item>
    ///     </list>
    /// </item>
    /// </list>
    /// 
    /// Note that the configuration did not have to use two-dimensional arrays; 
    /// however, there would be greater verbosity in the configuration file 
    /// if one used an array of ChildClaim objects, rather than a two-dimensional 
    /// array.
    /// </summary>
    public class ChildClaimCache : IChildClaimCache {


        /// <summary>
        /// Constructs a new ChildClaimCache object, populating
        /// the child claims with data retrieved from ChildClaims()
        /// </summary>
        public ChildClaimCache(IOptionsMonitor<ChildClaimSettings> settings) {
            ChildClaims = settings.CurrentValue.GetChildClaims();
        }

        public IEnumerable<ChildClaim> ChildClaims { get; }
    }
}
