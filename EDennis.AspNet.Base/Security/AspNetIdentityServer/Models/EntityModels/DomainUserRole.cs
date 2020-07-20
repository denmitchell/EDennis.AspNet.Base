using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainUserRoleJsonConverter))]
    public class DomainUserRole : IdentityUserRole<Guid>, ITemporalEntity {

        public DomainUser User { get; set; }
        public DomainRole Role { get; set; }

        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true)
            => DeserializeInto(jsonElement, this, modelState);


        public void Update(object updated) {
            var obj = updated as DomainUserRole;
            UserId = obj.UserId;
            RoleId = obj.RoleId;
            SysUser = obj.SysUser;
            SysStatus = obj.SysStatus;
            SysStart = obj.SysStart;
            SysEnd = obj.SysEnd;
        }

        public static DomainUserRole DeserializeInto(JsonElement jsonElement, DomainUserRole role, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "UserId":
                        case "userId":
                            role.UserId = prop.Value.GetGuid();
                            break;
                        case "RoleId":
                        case "roleId":
                            role.RoleId = prop.Value.GetGuid();
                            break;
                        case "SysUser":
                        case "sysUser":
                            role.SysUser = prop.Value.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            role.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            role.SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            role.SysEnd = prop.Value.GetDateTime();
                            break;
                        default:
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainUserRole).Name} JSON");
                }

            }
            return role;

        }

    }

    /// <summary>
    /// Explicitly define JsonConverter to prevent circular referencing during Serialization
    /// </summary>
    public class DomainUserRoleJsonConverter : JsonConverter<DomainUserRole> {

        public override DomainUserRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUserRole>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainUserRole value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("UserId", value.UserId.ToString());
                writer.WriteString("RoleId", value.RoleId.ToString());
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
            }
            writer.WriteEndObject();
        }
    }

}
