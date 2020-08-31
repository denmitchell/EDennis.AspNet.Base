using System;
using System.ComponentModel.DataAnnotations;

namespace EDennis.NetStandard.Base {
    public class DomainUserHistory {

        public int Id { get; set; }
        public DateTime DateReplaced { get; set; }

        [MaxLength(256)]
        public string ReplacedBy { get; set; }

        [MaxLength(256)]
        public string UserName { get; set; }

        [MaxLength(256)]
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public DateTimeOffset? LockoutBegin { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        [MaxLength(128)]
        public string Organization { get; set; }
        public bool OrganizationConfirmed { get; set; }
        public bool OrganizationAdmin { get; set; }
        public string UserClaims { get; set; }

    }
}
