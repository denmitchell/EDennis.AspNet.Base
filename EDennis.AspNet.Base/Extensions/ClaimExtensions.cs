using EDennis.AspNet.Base.Security;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base {
    public static class ClaimExtensions {

        public static Dictionary<string, string[]> ToDictionary(this IEnumerable<DomainUserClaim> claims) {
            return claims.GroupBy(c => new { c.ClaimType })
            .Select(g => new { Type = g.Key.ClaimType, Value = g.Select(i => i.ClaimValue).ToArray() })
                .ToDictionary(d => d.Type, d => d.Value);
        }

        public static Dictionary<string, string[]> ToDictionary(this IEnumerable<DomainRoleClaim> claims) {
            return claims.GroupBy(c => new { c.ClaimType })
            .Select(g => new { Type = g.Key.ClaimType, Value = g.Select(i => i.ClaimValue).ToArray() })
                .ToDictionary(d => d.Type, d => d.Value);
        }


        public static ICollection<DomainUserClaim> ToUserClaims(this  Dictionary<string, string[]> claims) {
            return claims.SelectMany(c => c.Value,
                (type, value) => new DomainUserClaim { ClaimType = type.Key, ClaimValue = value }).ToList();
        }

        public static ICollection<DomainRoleClaim> ToRoleClaims(this Dictionary<string, string[]> claims) {
            return claims.SelectMany(c => c.Value,
                (type, value) => new DomainRoleClaim { ClaimType = type.Key, ClaimValue = value }).ToList();
        }


    }
}
