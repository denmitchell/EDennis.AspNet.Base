using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    [JsonConverter(typeof(DomainUserRoleJsonConverter))]
    public class DomainUserRole : DomainUserRole<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {

        public override void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                switch (prop.Name) {
                    case "UserId":
                    case "userId":
                        UserId = prop.Value.GetGuid();
                        break;
                    case "RoleId":
                    case "roleId":
                        RoleId = prop.Value.GetGuid();
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

        public override void Update(object updated) {
            var entity = updated as DomainUserRole;
            UserId = entity.UserId;
            RoleId = entity.RoleId;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
        }
    }

    public class DomainUserRoleJsonConverter : JsonConverter<DomainUserRole> {

        public override DomainUserRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUserRole>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainUserRole value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("UserId", value.UserId.ToString());
                writer.WriteString("RoleId", value.RoleId.ToString());
                if (value.User != null && value.Role != null && value.Role.Application != null) {
                    writer.WriteString("UserName", value.User.UserName);
                    writer.WriteString("ApplicationName", value.Role.Application.Name);
                    writer.WriteString("RoleName", value.Role.Name);
                }
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
            }
            writer.WriteEndObject();
        }
    }


}