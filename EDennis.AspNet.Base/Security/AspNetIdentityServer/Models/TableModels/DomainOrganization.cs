using System;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainOrganizationJsonConverter))]
    public class DomainOrganization : IDomainEntity, IHasStringProperties {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }
        public string Properties { get; set; }

    }
}
