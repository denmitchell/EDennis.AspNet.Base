using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserClaim : IdentityUserClaim<Guid>, ITemporalEntity {

        [MaxLength(8000)]
        public string OtherProperties { get; set; }

        public DomainUser User { get; set; }


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
