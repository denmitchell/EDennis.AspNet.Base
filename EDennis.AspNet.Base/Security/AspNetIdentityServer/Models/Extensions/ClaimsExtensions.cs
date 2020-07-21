using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EDennis.AspNet.Base.Security {
    public static class ClaimsExtensions {
        public static IEnumerable<Claim> ToClaims<E>(this Dictionary<string, E> dict)
        where E : IEnumerable<string> {
            var list = new List<Claim>();
            foreach (var entry in dict) {
                foreach (var item in entry.Value)
                    list.Add(new Claim(entry.Key, item));
            }
            return list;
        }
    }
}
