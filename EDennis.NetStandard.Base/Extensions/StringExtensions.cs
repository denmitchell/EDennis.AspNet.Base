using System;


namespace EDennis.NetStandard.Base {
    public static class StringExtensions {

        public static string SafeSubstring(this string str, int startIndex, int maxLength) {
            if (str == null || startIndex > str.Length - 1)
                return null;
            var length = Math.Min(str.Length, maxLength);
            return str.Substring(startIndex, length);
        }


        public static bool MatchesWildcardPattern(this string source, string pattern) {

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

}
