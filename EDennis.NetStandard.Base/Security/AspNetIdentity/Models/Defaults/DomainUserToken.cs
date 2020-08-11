using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    [JsonConverter(typeof(DomainUserTokenJsonConverter))]
    public class DomainUserToken : DomainUserToken<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        public override void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                switch (prop.Name) {
                    case "UserId":
                    case "userId":
                        UserId = prop.Value.GetInt32();
                        break;
                    case "LoginProvider":
                    case "loginProvider":
                        LoginProvider = prop.Value.GetString();
                        break;
                    case "Name":
                    case "name":
                        Name = prop.Value.GetString();
                        break;
                    case "Value":
                    case "value":
                        Value = prop.Value.GetString();
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
            var entity = updated as DomainUserToken;
            UserId = entity.UserId;
            LoginProvider = entity.LoginProvider;
            Name = entity.Name;
            Value = entity.Value;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
        }
    }

    public class DomainUserTokenJsonConverter : JsonConverter<DomainUserToken> {

        public override DomainUserToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUserToken>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainUserToken value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteNumber("UserId", value.UserId);
                writer.WriteString("LoginProvider", value.LoginProvider.ToString());
                writer.WriteString("Name", value.Name.ToString());
                writer.WriteString("Value", value.Value.ToString());
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
            }
            writer.WriteEndObject();
        }
    }
}
