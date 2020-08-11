using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    [JsonConverter(typeof(DomainUserLoginJsonConverter))]
    public class DomainUserLogin: DomainUserLogin<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
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
                    case "ProviderKey":
                    case "providerKey":
                        ProviderKey = prop.Value.GetString();
                        break;
                    case "ProviderDisplayName":
                    case "providerDisplayName":
                        ProviderDisplayName = prop.Value.GetString();
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
            var entity = updated as DomainUserLogin;
            UserId = entity.UserId;
            LoginProvider = entity.LoginProvider;
            ProviderKey = entity.ProviderKey;
            ProviderDisplayName = entity.ProviderDisplayName;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
        }
    }

    public class DomainUserLoginJsonConverter : JsonConverter<DomainUserLogin> {

        public override DomainUserLogin Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUserLogin>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainUserLogin value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteNumber("UserId", value.UserId);
                writer.WriteString("LoginProvider", value.LoginProvider.ToString());
                writer.WriteString("ProviderKey", value.ProviderKey.ToString());
                writer.WriteString("ProviderDisplayName", value.ProviderDisplayName.ToString());
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
            }
            writer.WriteEndObject();
        }
    }

}
