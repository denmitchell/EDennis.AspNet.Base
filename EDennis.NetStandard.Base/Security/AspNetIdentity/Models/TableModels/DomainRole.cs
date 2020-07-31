using Microsoft.AspNetCore.Identity;
using System;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    [JsonConverter(typeof(DomainRoleJsonConverter))]
    public class DomainRole : IdentityRole<Guid>, IDomainEntity, IHasStringProperties {

        public virtual Guid? ApplicationId { get; set; }
        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }

        public string Properties { get; set; }

    }
}
