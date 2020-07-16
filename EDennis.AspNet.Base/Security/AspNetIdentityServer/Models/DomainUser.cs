using Microsoft.AspNetCore.Identity;
using System;
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
    }
}
