using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

namespace EDennis.NetStandard.Base {
    public static class UserClaimExtensions<TUserClaim>
        where TUserClaim : IdentityUserClaim<int>, new(){

        public static Dictionary<string, string[]> ToDictionary(IEnumerable<TUserClaim> claims) {
            return claims.GroupBy(c => new { c.ClaimType })
            .Select(g => new { Type = g.Key.ClaimType, Value = g.Select(i => i.ClaimValue).ToArray() })
                .ToDictionary(d => d.Type, d => d.Value);
        }

        public static ICollection<TUserClaim> ToUserClaims(Dictionary<string, string[]> claims) {
            return claims.SelectMany(c => c.Value,
                (type, value) => new TUserClaim { ClaimType = type.Key, ClaimValue = value }).ToList();
        }


    }

}