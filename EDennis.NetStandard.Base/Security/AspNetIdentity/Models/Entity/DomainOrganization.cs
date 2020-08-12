using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public class DomainOrganization : ICrudEntity {

        public string Name { get; set; }

        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }

        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                switch (prop.Name) {
                    case "Name":
                    case "name":
                        Name = prop.Value.GetString();
                        break;
                    case "SysUser":
                    case "sysUser":
                        SysUser = prop.Value.GetString();
                        break;
                    case "SysStatus":
                    case "sysStatus":
                        SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                        break;
                }
            }
        }

        public void Update(object updated) {
            var entity = updated as DomainOrganization;
            Name = entity.Name;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
        }


    }
}
