using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDennis.AspNet.Base.Security {
    public class DomainUser : IdentityUser<Guid> {

        public Guid OrganizationId { get; set; }

        public DateTime? LockoutBegin { get; set; }

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

        public string OtherProperties { get; set; }

        public DomainOrganization Organization { get; set; }
        public ICollection<DomainUserClaim> UserClaims { get; set; }
        public ICollection<DomainUserRole> UserRoles { get; set; }
        public ICollection<DomainUserLogin> UserLogins { get; set; }
        public ICollection<DomainUserLogin> UserTokens { get; set; }

    }
}
