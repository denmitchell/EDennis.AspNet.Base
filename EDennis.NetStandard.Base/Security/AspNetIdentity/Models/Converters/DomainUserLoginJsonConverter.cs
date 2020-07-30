using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base.Security {
    /// <summary>
    /// Explicitly define JsonConverter to prevent circular referencing during Serialization
    /// </summary>
    public class DomainUserLoginJsonConverter : JsonConverter<DomainUserLogin> {

        public override DomainUserLogin Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUserLogin>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainUserLogin value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("UserId", value.UserId.ToString());
                writer.WriteString("LoginProvider", value.LoginProvider);
                writer.WriteString("ProviderKey", value.ProviderKey);
                writer.WriteString("ProviderDisplayName", value.ProviderDisplayName);
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
            }
            writer.WriteEndObject();
        }
    }

}
