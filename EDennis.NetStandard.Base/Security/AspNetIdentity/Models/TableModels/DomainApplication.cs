using System;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base.Security {

    [JsonConverter(typeof(DomainApplicationJsonConverter))]
    public class DomainApplication : IDomainEntity, IHasStringProperties {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }
        public string Properties { get; set; }

    }
}
