using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace EDennis.AspNet.Base.Security {

    /// <summary>
    /// Are these still needed, or can we use EDennis.AspNet.Base.ClaimExtensions?
    /// </summary>
    public static class ClaimsExtensions {
        public static IEnumerable<Claim> ToClaimEnumerable<E>(this Dictionary<string, E> dict)
        where E : IEnumerable<string> {
            var list = new List<Claim>();
            foreach (var entry in dict) {
                foreach (var item in entry.Value)
                    list.Add(new Claim(entry.Key, item));
            }
            return list;
        }

        public static Dictionary<string,string[]> ToStringDictionary(this IEnumerable<Claim> claims) {
            return claims.GroupBy(c => new { c.Type })
             .Select(g => new { g.Key.Type, Value = g.Select(i => i.Value).ToArray() })
                 .ToDictionary(d => d.Type, d => d.Value);
        }

    }
}
