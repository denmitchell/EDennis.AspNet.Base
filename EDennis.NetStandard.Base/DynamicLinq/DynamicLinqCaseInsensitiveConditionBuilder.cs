
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Exceptions;
using System.Text.RegularExpressions;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Helper class for removing and applying string comparisons --
    /// StartsWith, EndsWith, Contains, and Like -- to a supplied 
    /// dynamic linq where string.  The resulting query expressions are
    /// translated into SQL LIKE clauses.
    /// SPECIAL NOTE: If multiple conditions are applied, it only handles
    /// AND-ed conditions
    /// </summary>
    /// <typeparam name="T">Underlying entity/class type</typeparam>
    public static class DynamicLinqCaseInsensitiveConditionBuilder<T>
        where T : class {

        public static string STARTSWITH_PATTERN = ".StartsWith(";
        public static string ENDSWITH_PATTERN = ".EndsWith(";
        public static string CONTAINS_PATTERN = ".Contains(";
        public static string LIKE_PATTERN = ".Like(";

        public static readonly Regex StartsWithRegex = new Regex("(?<and1>\\s*and\\s*)?(?<property>[A-Za-z0-9_]*).StartsWith\\(\"?(?<pattern>[^\")]*)\"?\\)(?<and2>\\s*and\\s*)?", RegexOptions.IgnoreCase);
        public static readonly Regex EndsWithRegex = new Regex("(?<and1>\\s*and\\s*)?(?<property>[A-Za-z0-9_]*).EndsWith\\(\"?(?<pattern>[^\")]*)\"?\\)(?<and2>\\s*and\\s*)?", RegexOptions.IgnoreCase);
        public static readonly Regex ContainsRegex = new Regex("(?<and1>\\s*and\\s*)?(?<property>[A-Za-z0-9_]*).Contains\\(\"?(?<pattern>[^\")]*)\"?\\)(?<and2>\\s*and\\s*)?", RegexOptions.IgnoreCase);
        public static readonly Regex LikeRegex = new Regex("(?<and1>\\s*and\\s*)?(?<property>[A-Za-z0-9_]*).Like\\(\"?(?<pattern>[^\")]*)\"?\\)(?<and2>\\s*and\\s*)?", RegexOptions.IgnoreCase);

        public static Dictionary<string, (Regex Regex, Func<IQueryable<T>, string, string, IQueryable<T>> Method)> _dict;

        static DynamicLinqCaseInsensitiveConditionBuilder() {
            _dict = new Dictionary<string, (Regex Regex, Func<IQueryable<T>, string, string, IQueryable<T>> Method)> {
                { STARTSWITH_PATTERN, (StartsWithRegex, IQueryableExtensions_DynamicLinqStringSearch.WhereStartsWith) },
                { ENDSWITH_PATTERN, (EndsWithRegex, IQueryableExtensions_DynamicLinqStringSearch.WhereEndsWith) },
                { CONTAINS_PATTERN, (ContainsRegex, IQueryableExtensions_DynamicLinqStringSearch.WhereContains) },
                { LIKE_PATTERN, (LikeRegex, IQueryableExtensions_DynamicLinqStringSearch.WhereLike) }
            };
        }


        /// <summary>
        /// Apply zero or more StartsWith, EndsWith, Contains, and Like conditions to a where
        /// clause, removing the conditions from the string
        /// SPECIAL NOTE: If multiple conditions are applied, it only handles
        /// AND-ed conditions
        /// </summary>
        /// <param name="qry"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static (IQueryable<T> Query, string Where) ParseApplyStringConditions(IQueryable<T> qry, string where) {

            foreach(var key in _dict.Keys) {
                if (where.Contains(key, StringComparison.OrdinalIgnoreCase)) {

                    var regex = _dict[key].Regex;
                    var method = _dict[key].Method;

                    var matches = regex.Matches(where);

                    if (matches.Count == 0)
                        throw new ParseException($"Unable to parse {key} clause(s) in {where}", where.IndexOf(key, StringComparison.OrdinalIgnoreCase));

                    foreach (Match match in matches) {

                        (bool and1, string property, string pattern, bool and2) 
                            = (match.Groups["and1"].Success, match.Groups["property"].Value, 
                                match.Groups["pattern"].Value, match.Groups["and2"].Success);

                        var replaceWith = "";
                        if (and1 && and2)
                            replaceWith = " and ";

                        where = regex.Replace(where, replaceWith);
                        qry = method(qry, property, pattern);
                    }
                }
            }

            return (qry, where);

        }



    }
}
