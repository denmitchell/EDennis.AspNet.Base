using Microsoft.AspNetCore.Identity;

namespace EDennis.AspNet.Base.Security {
    public class DomainRole : IdentityRole {

        public virtual int? OrganizationId { get; set; }
        public virtual int? ApplicationId { get; set; }
        public virtual string RoleName { get; set; }

        //Name property = 
        //Admin@CT.DDS.PRAT -- {RoleName}@{ApplicationName}
        //Admin@Oak Hill -- {RoleName}@{OrganizationName}
        //Admin@Oak Hill@CT.DDS.PRAT
    }
}
