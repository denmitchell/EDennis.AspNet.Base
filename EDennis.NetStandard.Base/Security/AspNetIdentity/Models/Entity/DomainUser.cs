using IdentityModel;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace EDennis.NetStandard.Base {
    public class DomainUser : IdentityUser<int> {

        public const int SHA256_LENGTH = 64; //"a8a2f6ebe286697c527eb35a58b5539532e9b3ae3b64d4eb0a46fb657b41562c";
        public const int SHA512_LENGTH = 128; //"f3bf9aa70169e4ab5339f20758986538fe6c96d7be3d184a036cde8161105fcf53516428fa096ac56247bb88085b0587d5ec8e56a6807b1af351305b2103d74b";


        public override bool LockoutEnabled { get; set; }

        public DateTimeOffset? _lockoutBegin;
        public DateTimeOffset? _lockoutEnd;


        public DateTimeOffset? LockoutBegin {
            get => _lockoutBegin;
            set {
                _lockoutBegin = value;
                if (_lockoutBegin <= DateTime.Now && _lockoutEnd > DateTime.Now)
                    LockoutEnabled = true;
                else
                    LockoutEnabled = false;
            }
        }

        public override DateTimeOffset? LockoutEnd {
            get => _lockoutEnd;
            set {
                _lockoutEnd = value;
                if (_lockoutBegin <= DateTime.Now && _lockoutEnd > DateTime.Now)
                    LockoutEnabled = true;
                else
                    LockoutEnabled = false;
            }            
        }

        public string Organization { get; set; }

        /// <summary>
        /// Transforms user properties into claims
        /// </summary>
        /// <returns></returns>
        public virtual ICollection<Claim> ToClaims() =>
            new List<Claim> {
                new Claim(JwtClaimTypes.Name, UserName),
                new Claim(ClaimTypes.Name, UserName),
                new Claim(JwtClaimTypes.Email, Email),
                new Claim(DomainClaimTypes.Organization, Organization)
            };
    }
}
