using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {


    [JsonConverter(typeof(DomainUserClaimJsonConverter))]
    public class DomainUserClaim : IdentityUserClaim<Guid>, IDomainEntity, IHasStringProperties {

        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }

        public string Properties { get; set; }
        public DomainUser User { get; set; }

    }

}
