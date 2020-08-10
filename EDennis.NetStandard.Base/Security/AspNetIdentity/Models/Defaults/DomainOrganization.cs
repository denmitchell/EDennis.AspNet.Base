using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {


    [JsonConverter(typeof(DomainRoleJsonConverter))]
    public class DomainOrganization : DomainOrganization<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        public override void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                switch (prop.Name) {
                    case "Id":
                    case "id":
                        Id = prop.Value.GetGuid();
                        break;
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

        public override void Update(object updated) {
            var entity = updated as DomainOrganization;
            Id = entity.Id;
            Name = entity.Name;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
        }
    }

    public class DomainOrganizationJsonConverter : JsonConverter<DomainOrganization> {

        public override DomainOrganization Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            JsonSerializer.Deserialize<DomainOrganization>(ref reader, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        public override void Write(Utf8JsonWriter writer, DomainOrganization value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("Name", value.Name);
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
            }
            writer.WriteEndObject();
        }
    }

}