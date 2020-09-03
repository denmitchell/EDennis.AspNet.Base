using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDennis.NetStandard.Base.Security.DefaultPolicies {


    /// <summary>
    /// Legacy pattern matching utility that was once used by the 
    /// ClaimPatternAuthorizationHandler for matching scopes to policies.
    /// 
    /// This legacy pattern matcher does not use regular expressions.  It may
    /// be slightly more performant than the new PatternMatcher, but it is
    /// much more complicated -- harder to understand and harder to maintain.
    /// It is included just in case the performance of the new PatternMatcher
    /// is not acceptable or in case there are some edge cases where the
    /// two pattern matching algorithms return different results.
    /// </summary>
    public static class LegacyPatternMatcher {

        public const char EXCLUSION_PREFIX = '-';
        public const char SUBPATTERN_DELIMITER = ',';

        /// <summary>
        /// Evaluates an individual scope claim against a requirement scope
        /// </summary>
        /// <param name="requirement">the policy requirement scope</param>
        /// <param name="scopeClaim">the user/client's scope claim</param>
        /// <returns></returns>
        public static bool Matches(string source, string pattern) {

            MatchContext context = pattern.StartsWith(EXCLUSION_PREFIX) ? MatchContext.Exclusion : MatchContext.Inclusion;

            //split each scope into a set of patterns
            var patterns = pattern.Split(SUBPATTERN_DELIMITER).Select(x => x.Trim());

            //handle special if just one pattern in scope claim and inclusion
            if (patterns.Count() == 1 && context == MatchContext.Inclusion)
                if (patterns.ElementAt(0) == "*")
                    return true;
                else
                    return MatchesWildcardPattern(source, patterns.ElementAt(0));


            //keeps track of how each character in the requirement scope matches
            //initialize with wildcard matches of the other type
            MatchType[] characterMatches = Enumerable.Repeat(
                    (context == MatchContext.Inclusion)
                        ? MatchType.WildcardExclusion
                        : MatchType.WildcardInclusion, source.Length).ToArray();


            //evaluate each pattern
            foreach (var p in patterns)
                EvaluatePattern(source, p, ref characterMatches);


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
        public static void EvaluatePattern(string source, string patternPossiblyWithPrefix,
            ref MatchType[] characterMatches) {

            MatchContext context = patternPossiblyWithPrefix.StartsWith(EXCLUSION_PREFIX) ? MatchContext.Exclusion : MatchContext.Inclusion;
            var pattern = patternPossiblyWithPrefix.Substring(context == MatchContext.Exclusion ? 1 : 0);

            MatchType[] matches = new MatchType[characterMatches.Length];
            Array.Copy(characterMatches, matches, characterMatches.Length);

            MatchType wildcardForContext = (context == MatchContext.Inclusion) ? MatchType.WildcardInclusion : MatchType.WildcardExclusion;
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

        public static bool MatchesWildcardPattern(string source, string pattern) {

            //short-circuit for trivial results;
            if (source == pattern)
                return true;
            else if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
                return false;
            else if (pattern == "*")
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
                else if (s < source.Length && p < pattern.Length && source[s] == pattern[p])
                    continue;

                //handle asterix in pattern
                else if (p < pattern.Length && pattern[p] == '*') {

                    //advance through pattern string until non-asterix character is encountered or end of string
                    while (p < pattern.Length && pattern[p] == '*')
                        p++;

                    //if end of pattern is reached and it ends with '*', it matches
                    if (p == pattern.Length)
                        return true;

                    //advance the source to the first character that matches the pattern's next (non-asterix) character
                    while (s < source.Length && source[s] != pattern[p])
                        s++;

                    //corresponding characters don't match and pattern character isn't an asterix; so, non-match
                } else
                    return false;
            }

            //with the asterix-consuming feature of the above loop, 
            //at this point, the only way the input string matches the pattern 
            //is if there are no more characters remaining to match from 
            //both the input and pattern.
            return s == source.Length && p == pattern.Length;


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
