using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    public class DomainUserClaimJsonConverter : JsonConverter<DomainUserClaim> {

        public override DomainUserClaim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
            => JsonSerializer.Deserialize<DomainUserClaim>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainUserClaim value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("ClaimType", value.ClaimType);
                writer.WriteString("ClaimValue", value.ClaimValue);
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
            }
            writer.WriteEndObject();
        }
    }
}
