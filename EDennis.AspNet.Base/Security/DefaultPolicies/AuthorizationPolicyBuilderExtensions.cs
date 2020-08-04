using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace EDennis.NetStandard.Base {
    public static class AuthorizationPolicyBuilderExtensions {

        /// <summary>
        /// Adds a <see cref="PositiveClaimsAuthorizationRequirement"/>
        /// to the current instance.
        /// </summary>
        /// <param name="claimType">The claim type that must be present and, if specified, have one ore more allowed values.</param>
        /// <param name="allowedValues">Values the claim must process one or more of for evaluation to succeed.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static AuthorizationPolicyBuilder RequireClaimPatternMatch(
            this AuthorizationPolicyBuilder builder, string requirementScope,
            ConcurrentDictionary<string, bool> policyPatternCache, ILogger logger) {
            if (requirementScope == null) {
                throw new ArgumentNullException(nameof(requirementScope));
            }

            builder.Requirements.Add(new ClaimPatternAuthorizationHandler(requirementScope, policyPatternCache, logger));
            return builder;
        }


    }
}