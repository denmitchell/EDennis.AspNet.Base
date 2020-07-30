using System;
namespace EDennis.NetStandard.Base.Security {
    public class ExpandedDomainRole {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }
        public string Properties { get; set; }

    }
}
