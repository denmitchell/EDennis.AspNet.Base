
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security {
    public class DomainApplication : ITemporalEntity {
        public Guid Id { get; set; } = CombGuid.Create();
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; }
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
