using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {
    public class DomainUser : IdentityUser<int> {


        public string Organization { get; set; }
        public bool OrganizationConfirmed { get; set; }
        public bool OrganizationAdmin { get; set; }


        #region User Locking

        public DateTimeOffset? LockoutBegin { get; set; }


        //make LockoutEnabled a calculated field
        [NotMapped]
        public override bool LockoutEnabled {
            get {
                return LockoutBegin <= DateTime.Now && LockoutEnd >= DateTime.Now;
            } 
            set { }
        }


        #endregion

        /// <summary>
        /// Transforms user properties into claims
        /// </summary>
        /// <returns></returns>
        public virtual ICollection<Claim> ToClaims() {
            var claims = 
            new List<Claim> {
                new Claim(JwtClaimTypes.Subject, Id.ToString()),
                new Claim(JwtClaimTypes.Name, UserName),
                new Claim(ClaimTypes.Name, UserName),
            };
            if(Email != default) {
                claims.Add(new Claim(JwtClaimTypes.Email, Email));
                claims.Add(new Claim(JwtClaimTypes.EmailVerified, EmailConfirmed.ToString().ToLower()));
            }
            if (PhoneNumber != default) {
                claims.Add(new Claim(JwtClaimTypes.PhoneNumber, PhoneNumber));
                claims.Add(new Claim(JwtClaimTypes.PhoneNumberVerified, PhoneNumberConfirmed.ToString().ToLower()));
            }
            if (Organization != default) {
                claims.Add(new Claim(DomainClaimTypes.Organization, Email));
                claims.Add(new Claim(DomainClaimTypes.OrganizationConfirmed, OrganizationConfirmed.ToString().ToLower()));
                claims.Add(new Claim(DomainClaimTypes.OrganizationAdmin, OrganizationAdmin.ToString().ToLower()));
            }
            if (LockoutBegin <= DateTime.Now && LockoutEnd >= DateTime.Now)
                claims.Add(new Claim(DomainClaimTypes.Locked, "true"));

            return claims;
        }

    }
}
