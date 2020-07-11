using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace EDennis.AspNet.Base.Security {
    public class DomainRole : IdentityRole<Guid> {

        [RegularExpression(@"[U|C|A|O]")]
        public virtual char AppliesTo { get; set; } = 'U';
        public virtual Guid? OrganizationId { get; set; }
        public virtual Guid? ApplicationId { get; set; }
        public virtual string Title { get; set; }

    }
}
