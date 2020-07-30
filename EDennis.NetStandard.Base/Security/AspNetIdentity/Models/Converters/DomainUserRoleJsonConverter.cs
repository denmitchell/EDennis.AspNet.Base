using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base.Security {

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
