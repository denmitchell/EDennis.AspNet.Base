using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    /// NOTE: Breaking change: Remove support for "user_scope" -- now just use
    /// "scope".  To obtain scope claims from a user, assign the user to a role 
    /// and specify role-associated claims in the target application, which are
    /// loaded into the DomainRoleClaimCache and retrieved with the
    /// DomainRoleClaimsTransformer
    /// 
    /// NOTE: This is adapted from ... https://github.com/aspnet/Security/blob/master/src/Microsoft.AspNetCore.Authorization/Infrastructure/ClaimsAuthorizationRequirement.cs
    /// 
    /// <see cref="DomainRoleClaimCache"/>
    /// <see cref="DomainRoleClaimsTransformer"/>
    /// </summary>
    public class ClaimPatternAuthorizationHandler : AuthorizationHandler<ClaimPatternAuthorizationHandler>, IAuthorizationRequirement {
        /// <summary>
        /// Creates a new instance of <see cref="ClaimPatternAuthorizationHandler"/>.
        /// </summary>
        /// <param name="claimType">The claim type that must be absent if no values are provided.</param>
        /// <param name="AllowedValues">The optional list of claim values, which, if present, 
        /// the claim must NOT match.</param>
        public ClaimPatternAuthorizationHandler(
                string requirementScope, ConcurrentDictionary<string, bool> policyPatternCache,
                ILogger logger, string claimType ) {

            RequirementScope = requirementScope;
            PolicyPatternCache = policyPatternCache;
            Logger = logger;
            ClaimType = claimType;
        }


        /// <summary>
        /// Gets the scope/policy value a scope claim pattern must match
        /// </summary>
        public string RequirementScope { get; }

        public ILogger Logger { get; set; }


        public string ClaimType;

        /// <summary>
        /// NOTE: Exclusions are evaluated after all included scopes.
        /// NOTE: When only exclusions are present, application-level scope
        ///       is used as the base from which exclusions are applied.
        /// </summary>
        public const string EXCLUSION_PREFIX = "-";

        //within a singleton, holds all previously matched patterns
        //that indicate success or failure against the policy
        public ConcurrentDictionary<string, bool> PolicyPatternCache { get; set; }


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
                    if (PolicyPatternCache.ContainsKey(scopeClaim)) {
                        isSuccess = PolicyPatternCache[scopeClaim];
                        Logger.LogTrace("For default policy requirement {PolicyRequirement}, Scope claim pattern {ScopeClaim} is cached, returning {Result}", RequirementScope, scopeClaim, isSuccess);

                        //otherwise, evaluate the scope's pattern(s)
                    } else {
                        Logger.LogTrace("For default policy requirement {PolicyRequirement}, evaluating {ScopeClaim} pattern(s)", RequirementScope, scopeClaim, isSuccess);
                        isSuccess = EvaluateScopeClaim(handler.RequirementScope, scopeClaim);
                        PolicyPatternCache.TryAdd(scopeClaim, isSuccess); //add to cache
                    }

                    //short-circuit if success
                    if (isSuccess) {
                        Logger.LogTrace("For default policy requirement {PolicyRequirement}, Scope claim pattern {ScopeClaim} matches, returning {Result}", RequirementScope, scopeClaim, isSuccess);
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
        public bool EvaluateScopeClaim(string requirement, string scopeClaim) {

            MatchContext context = scopeClaim.StartsWith(EXCLUSION_PREFIX) ? MatchContext.Exclusion : MatchContext.Inclusion;

            //split each scope into a set of patterns
            var patterns = scopeClaim.Split(',').Select(x=>x.Trim());

            //handle special if just one pattern in scope claim and inclusion
            if (patterns.Count() == 1 && context == MatchContext.Inclusion)
                if (patterns.ElementAt(0) == "*")
                    return true;
                else
                    return requirement.MatchesWildcardPattern(patterns.ElementAt(0));


            //keeps track of how each character in the requirement scope matches
            //initialize with wildcard matches of the other type
            MatchType[] characterMatches = Enumerable.Repeat(                 
                    (context==MatchContext.Inclusion) 
                        ? MatchType.WildcardExclusion 
                        : MatchType.WildcardInclusion, requirement.Length).ToArray();


            //evaluate each pattern
            foreach (var pattern in patterns)
                EvaluatePattern(requirement, pattern, ref characterMatches);


            //iterate over character matches, returning false if at least one character
            //is still unmatched or matched with exclusion pattern only 
            foreach (MatchType characterMatch in characterMatches)
                if (characterMatch == MatchType.NonWildcardExclusion || characterMatch == MatchType.WildcardExclusion)
                    return false;


            return true;
        }



        /// <summary>
        /// This method does nearly the same thing as the MatchesWildcardPattern method
        /// below; however, rather than returning an immediate result, it conditionally
        /// updates an array holding the type of match by each character.  The array
        /// is updated only when the entire pattern matches
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="pattern">pattern to use for matching</param>
        /// <param name="context">whether inclusion or exclusion</param>
        /// <param name="characterMatches">when more than one string is evaluated, nonStarMatches is used for reconciliation</param>
        /// <returns></returns>
        public void EvaluatePattern(string source, string patternPossiblyWithPrefix,
            ref MatchType[] characterMatches) {

            MatchContext context = patternPossiblyWithPrefix.StartsWith(EXCLUSION_PREFIX) ? MatchContext.Exclusion : MatchContext.Inclusion;
            var pattern = patternPossiblyWithPrefix.Substring(context == MatchContext.Exclusion ? 1 : 0);

            MatchType[] matches = new MatchType[characterMatches.Length]; 
            Array.Copy(characterMatches,matches,characterMatches.Length);

            MatchType wildcardForContext = (context==MatchContext.Inclusion) ? MatchType.WildcardInclusion : MatchType.WildcardExclusion;
            MatchType nonWildcardForContext = (context == MatchContext.Inclusion) ? MatchType.NonWildcardInclusion : MatchType.NonWildcardExclusion;


            //overwrite characterMatches, only if the overall pattern matches (inclusion or exclusion)
            if (Matches())
                characterMatches = matches;

            bool Matches() {
                //short-circuit for trivial results;
                if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
                    return true;

                //initialize string index variables
                int s = 0;
                int p = 0;

                //iterate over source and pattern characters until end of source
                //or pattern (short-circuiting if no match)
                for (; s < source.Length || p < pattern.Length; s++, p++) {

                    //return true if end of source and pattern
                    if (s == source.Length && p == pattern.Length)
                        return true;

                    //handle matching characters without wildcard
                    else if (s < source.Length && p < pattern.Length && source[s] == pattern[p]) {
                        //overwrite when lower value
                        if (matches[s] < nonWildcardForContext)
                            matches[s] = nonWildcardForContext;
                        continue;
                    }

                    //handle asterix in pattern
                    else if (p < pattern.Length && pattern[p] == '*') {

                        //advance through pattern string until non-asterix character is encountered or end of string
                        while (p < pattern.Length && pattern[p] == '*')
                            p++;


                        //advance the source to the first character that matches the pattern's next (non-asterix) character
                        while (s < source.Length && (p == pattern.Length || source[s] != pattern[p])) {
                            //overwrite unmatched only with wildcard match
                            if (matches[s] < wildcardForContext)
                                matches[s] = wildcardForContext;
                            s++;
                        }
                        //if end of pattern is reached and it ends with '*', it matches
                        if (p == pattern.Length)
                            return true;

                        //back up one, as the counter will advance p and s at the top of the loop
                        s--;
                        p--;
                        //corresponding characters don't match and pattern character isn't an asterix; so, non-match
                    } else
                        return false;
                }
                return s == source.Length && p == pattern.Length;

            }

        }

    }

    public enum MatchType : byte {
        WildcardExclusion = 0,
        WildcardInclusion = 1,
        NonWildcardExclusion = 2,
        NonWildcardInclusion = 3
    }

    public enum MatchContext {
        Exclusion = 0,
        Inclusion = 1
    }


}


