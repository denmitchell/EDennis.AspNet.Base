using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainRoleJsonConverter))]
    public class DomainRole : IdentityRole<Guid>, IDomainEntity, IHasStringProperties {
        public virtual Guid? ApplicationId { get; set; }

        public string Properties { get; set; }

        public DomainApplication Application { get; set; }

        public ICollection<DomainUserRole> UserRoles { get; set; }


        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void DeserializeInto(JsonElement source, ModelStateDictionary modelState) {
            bool hasWrittenProperties = false;
            using var ms = new MemoryStream();
            using var jw = new Utf8JsonWriter(ms);

            foreach (var prop in source.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Id":
                        case "id":
                            Id = prop.Value.GetGuid();
                            break;
                        case "ApplicationId":
                        case "applicationId":
                            ApplicationId = prop.Value.GetGuid();
                            break;
                        case "ConcurrencyStamp":
                        case "concurrencyStamp":
                            ConcurrencyStamp = prop.Value.GetString();
                            break;
                        case "Name":
                        case "name":
                            Name = prop.Value.GetString();
                            NormalizedName = Name.ToUpper();
                            break;
                        case "NormalizedName":
                        case "normalizedName":
                            NormalizedName = prop.Value.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            SysUser = prop.Value.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            SysEnd = prop.Value.GetDateTime();
                            break;
                        default:
                            if (!hasWrittenProperties)
                                jw.WriteStartObject();
                            prop.WriteTo(jw);
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {GetType().Name} JSON");
                }
                if (hasWrittenProperties) {
                    jw.WriteEndObject();
                    Properties = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

    }
}
