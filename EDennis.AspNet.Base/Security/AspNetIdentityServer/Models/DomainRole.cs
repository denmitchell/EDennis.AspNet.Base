using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security {
    public class DomainRole : IdentityRole<Guid>, ITemporalEntity {
        public virtual Guid? OrganizationId { get; set; }
        public virtual Guid? ApplicationId { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public ICollection<DomainUserRole> UserRoles { get; set; }

        public DomainApplication Application { get; set; }
        public DomainOrganization Organization { get; set; }


        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void Patch(JsonElement jsonElement) {
            throw new NotImplementedException();
        }

        public void Update(object updated) {
            throw new NotImplementedException();
        }


    }
}
