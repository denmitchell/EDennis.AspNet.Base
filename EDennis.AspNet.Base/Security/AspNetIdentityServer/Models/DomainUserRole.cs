using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserRole : IdentityUserRole<Guid>, ITemporalEntity {

        [MaxLength(8000)]
        public string OtherProperties { get; set; }

        public DomainUser User { get; set; }
        public DomainRole Role { get; set; }

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
