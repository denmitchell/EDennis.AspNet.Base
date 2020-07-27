using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainUserJsonConverter))]
    public class DomainUser : IdentityUser<Guid>, IDomainEntity, IHasStringProperties {

        public const int SHA256_LENGTH = 64; //"a8a2f6ebe286697c527eb35a58b5539532e9b3ae3b64d4eb0a46fb657b41562c";
        public const int SHA512_LENGTH = 128; //"f3bf9aa70169e4ab5339f20758986538fe6c96d7be3d184a036cde8161105fcf53516428fa096ac56247bb88085b0587d5ec8e56a6807b1af351305b2103d74b";
        public Guid OrganizationId { get; set; }

        public DateTimeOffset? LockoutBegin { get; set; }

        [NotMapped]
        public override bool LockoutEnabled {
            get => LockoutBegin <= DateTime.Now && LockoutEnd > DateTime.Now;
            set {
                if (value) {
                    LockoutBegin = DateTime.Now;
                    if (LockoutEnd == null || LockoutEnd < DateTime.Now)
                        LockoutEnd = DateTime.MaxValue;
                } else {
                    LockoutBegin = null;
                    LockoutEnd = null;
                }
            }
        }

        private string _passwordHash;


        public override string PasswordHash {
            get => _passwordHash;
            set {

                if (value.Length == SHA256_LENGTH || value.Length == SHA512_LENGTH)
                    _passwordHash = value;
                else
                    _passwordHash = value.Sha256();

            }
        }



        public string Properties { get; set; }

        public DomainOrganization Organization { get; set; }
        public ICollection<DomainUserClaim> UserClaims { get; set; }
        public Dictionary<string,string[]> UserClaims__Packed { get; set; }

        public ICollection<DomainUserRole> UserRoles { get; set; }
        public Dictionary<string,string[]> UserRoles__Packed { get; set; }

        public ICollection<DomainUserLogin> UserLogins { get; set; }
        public ICollection<DomainUserToken> UserTokens { get; set; }
        public DateTime SysEnd { get; set; }
        public DateTime SysStart { get; set; }
        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }


        
    }

}
