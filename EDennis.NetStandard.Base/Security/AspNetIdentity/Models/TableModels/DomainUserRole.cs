using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base.Security {

    [JsonConverter(typeof(DomainUserRoleJsonConverter))]
    public class DomainUserRole : IdentityUserRole<Guid>, IDomainEntity {

        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }

    }

}
