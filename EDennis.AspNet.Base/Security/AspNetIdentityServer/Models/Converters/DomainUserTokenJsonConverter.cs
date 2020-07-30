using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    public class DomainUserTokenJsonConverter : JsonConverter<DomainUserToken> {

        public override DomainUserToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUserToken>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainUserToken value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("UserId", value.UserId.ToString());
                writer.WriteString("LoginProvider", value.LoginProvider);
                writer.WriteString("Name", value.Name);
                writer.WriteString("Value", value.Value);
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
            }
            writer.WriteEndObject();
        }
    }

}
