using System.Linq;
using System.Text.RegularExpressions;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Pattern matching utility that is used by the 
    /// ClaimPatternAuthorizationHandler for matching 
    /// scopes to policies.
    /// </summary>
    public static class PatternMatcher {

        public const char EXCLUSION_PREFIX = '-';
        public const char SUBPATTERN_DELIMITER = ',';

        private static readonly Regex _allDots = new Regex("\\.");
        private const string ALL_DOTS_REPLACE_WITH = "`";

        private static readonly Regex _allButTerminalAsterisk = new Regex("\\*(?=.)");
        private const string ALL_BUT_TERMINAL_ASTERISK_REPLACE_WITH = ".+";

        private static readonly Regex _allAsterisks = new Regex("\\*");
        private const string ALL_ASTERISKS_REPLACE_WITH = ".*";

        private static readonly Regex _allBackticks = new Regex("`");
        private const string ALL_BACKTICKS_REPLACE_WITH = "\\.";

        /// <summary>
        /// Determines if the source string matches
        /// the provided pattern.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns>
        /// true if pattern = "*" or if the 
        /// longest matching subpattern in pattern 
        /// (when split by a comma) isn't a
        /// "negative exclusion"
        /// </returns>
        public static bool Matches(string source, string pattern) {
            if (pattern == "*") //special case
                return true;
            var patterns = pattern.Split(SUBPATTERN_DELIMITER).Select(x => x.Trim());
            (MatchType matchType, int NonWildCardLength) longestMatchingPattern = 
                (MatchType.NonMatch, -1);
            foreach(var p in patterns) {
                if(MatchesPattern(source, p, out MatchType matchType, out int nonWildCardLength))
                    if (nonWildCardLength > longestMatchingPattern.NonWildCardLength)
                        longestMatchingPattern = (matchType, nonWildCardLength);
            }
            //if no patterns match or if the longest matching pattern (not counting
            //   wildcard characters) has an ExclusionNegative match type,
            //   this will return true; otherwise, it will return false
            return longestMatchingPattern.matchType == MatchType.Inclusion 
                || longestMatchingPattern.matchType == MatchType.ExclusionPositive;
        }


        private enum MatchType {
            NonMatch,
            Inclusion,
            ExclusionPositive,
            ExclusionNegative
        }

        private static bool MatchesPattern(string source, string pattern, 
            out MatchType matchType, out int nonWildcardLength) {
            pattern = RewritePattern(pattern, out bool isExclusion, out nonWildcardLength);
            var matches = Regex.IsMatch(source, pattern);

            if (matches) {
                if (isExclusion)
                    matchType = MatchType.ExclusionNegative;
                else
                    matchType = MatchType.Inclusion;
            } else {
                if (isExclusion)
                    matchType = MatchType.ExclusionPositive;
                else
                    matchType = MatchType.NonMatch;
            }
            return matchType != MatchType.NonMatch;
        }

        private static string RewritePattern(string pattern, out bool isExclusion,
            out int nonWildcardLength) {

            if (pattern.StartsWith(EXCLUSION_PREFIX)) {
                isExclusion = true;
                pattern = pattern.Substring(1);
            } else
                isExclusion = false;


            nonWildcardLength = pattern.Where(x => x != '*').Count();

            pattern = _allDots.Replace(pattern, ALL_DOTS_REPLACE_WITH);
            pattern = _allButTerminalAsterisk.Replace(pattern, ALL_BUT_TERMINAL_ASTERISK_REPLACE_WITH);
            pattern = _allAsterisks.Replace(pattern, ALL_ASTERISKS_REPLACE_WITH);
            pattern = _allBackticks.Replace(pattern, ALL_BACKTICKS_REPLACE_WITH);

            return $"^{pattern}$";

        }


    }
}
