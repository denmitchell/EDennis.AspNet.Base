using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Implements an <see cref="IAuthorizationHandler"/> and <see cref="IAuthorizationRequirement"/>
    /// to evaluate claim patterns (scopes with wildcards) against policy requirements.
    /// 
    /// NOTE: Pass in the claim type that will be used to evaluate scope.  This could be
    /// "scope" or "client_scope" or "user_scope" or something else.  Typically, these
    /// values would be defined in Configuration (e.g., Security:DefaultPolicies:ClaimTypes:["scope"])
    /// 
    /// NOTE: This is adapted from ... https://github.com/aspnet/Security/blob/master/src/Microsoft.AspNetCore.Authorization/Infrastructure/ClaimsAuthorizationRequirement.cs
    /// </summary>
    public class ClaimPatternAuthorizationHandler : AuthorizationHandler<ClaimPatternAuthorizationHandler>, IAuthorizationRequirement {
        /// <summary>
        /// Creates a new instance of <see cref="ClaimPatternAuthorizationHandler"/>.
        /// </summary>
        /// <param name="claimType">The claim type that must be absent if no values are provided.</param>
        /// <param name="AllowedValues">The optional list of claim values, which, if present, 
        /// the claim must NOT match.</param>
        public ClaimPatternAuthorizationHandler(
                string requirementScope,
                string claimType) {

            RequirementScope = requirementScope;
            ClaimType = claimType;
        }


        /// <summary>
        /// Gets the scope/policy value a scope claim pattern must match
        /// </summary>
        public string RequirementScope { get; }


        public string ClaimType;

        //holds all previously matched patterns
        //that indicate success or failure against the policy
        private static readonly ConcurrentDictionary<string, bool> _policyPatternCache;

        static ClaimPatternAuthorizationHandler() {
            _policyPatternCache = new ConcurrentDictionary<string, bool>();
        }


        /// <summary>
        /// Makes a decision if authorization is allowed based on the claims requirements specified.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="handler">The requirement to evaluate.</param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ClaimPatternAuthorizationHandler handler) {

            bool? isSuccess = EvaluateClaimsPrincipal(context.User, handler);

            if (isSuccess == true) {
                context.Succeed(handler);
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// For a given claims principal (user or client), 
        /// evaluates all scope claims against the requirement scope,
        /// using cached results when possible.
        /// </summary>
        /// <param name="claimsPrincipal">user or client</param>
        /// <param name="handler">the current authorization handler</param>
        /// <returns></returns>
        public bool EvaluateClaimsPrincipal(ClaimsPrincipal claimsPrincipal, ClaimPatternAuthorizationHandler handler) {

            bool isSuccess = false;
            List<string> scopeClaims;

            //only process if there are any claims
            if (claimsPrincipal.Claims != null && claimsPrincipal.Claims.Count() > 0) {

                //get relevant claims (case-insensitve match on this)
                scopeClaims = claimsPrincipal.Claims
                        .Where(c => c.Type.Equals(ClaimType, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Value)
                        .ToList();

                //iterate over all scope claims
                foreach (var scopeClaim in scopeClaims) {

                    //if scope claim exists in cache, use the cache result
                    if (_policyPatternCache.ContainsKey(scopeClaim)) {
                        isSuccess = _policyPatternCache[scopeClaim];
                        System.Diagnostics.Debug.WriteLine($"For default policy requirement {RequirementScope}, Scope claim pattern {scopeClaim} is cached, returning {isSuccess}");

                        //otherwise, evaluate the scope's pattern(s)
                    } else {
                        System.Diagnostics.Debug.WriteLine($"For default policy requirement {RequirementScope}, evaluating {scopeClaim} pattern(s)");
                        isSuccess = EvaluateScopeClaim(handler.RequirementScope, scopeClaim);
                        _policyPatternCache.TryAdd(scopeClaim, isSuccess); //add to cache
                    }

                    //short-circuit if success
                    if (isSuccess) {
                        System.Diagnostics.Debug.WriteLine($"For default policy requirement {RequirementScope}, Scope claim pattern {scopeClaim} matches, returning {isSuccess}");
                        return true;
                    }
                }
            }

            return false; //if no successful scope claim, then false
        }


        /// <summary>
        /// Evaluates an individual scope claim against a requirement scope
        /// </summary>
        /// <param name="requirement">the policy requirement scope</param>
        /// <param name="scopeClaim">the user/client's scope claim</param>
        /// <returns></returns>
        public bool EvaluateScopeClaim(string requirement, string scopeClaim)
            => PatternMatcher.Matches(requirement, scopeClaim);
    }

}


