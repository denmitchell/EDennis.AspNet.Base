using System;


namespace EDennis.NetStandard.Base {
    public static class StringExtensions {

        public static string SafeSubstring(this string str, int startIndex, int maxLength) {
            if (str == null || startIndex > str.Length - 1)
                return null;
            var length = Math.Min(str.Length, maxLength);
            return str.Substring(startIndex, length);
        }


    }

}
