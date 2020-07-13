using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace EDennis.AspNet.Base.Security {
    public class DomainRole : IdentityRole<Guid> {
        public virtual Guid? OrganizationId { get; set; }
        public virtual Guid? ApplicationId { get; set; }

    }
}
