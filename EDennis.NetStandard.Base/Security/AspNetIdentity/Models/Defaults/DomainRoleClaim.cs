using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    [JsonConverter(typeof(DomainRoleClaimJsonConverter))]
    public class DomainRoleClaim : DomainRoleClaim<DomainUser, DomainOrganization, DomainUserClaim, DomainUserLogin, DomainUserToken, DomainRole, DomainApplication, DomainRoleClaim, DomainUserRole> {
        public override void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                switch (prop.Name) {
                    case "Id":
                    case "id":
                        Id = prop.Value.GetInt32();
                        break;
                    case "RoleId":
                    case "roleId":
                        RoleId = prop.Value.GetGuid();
                        break;
                    case "ClaimType":
                    case "claimType":
                        ClaimType = prop.Value.GetString();
                        break;
                    case "ClaimValue":
                    case "claimValue":
                        ClaimValue = prop.Value.GetString();
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
            var entity = updated as DomainRoleClaim;
            Id = entity.Id;
            RoleId = entity.RoleId;
            ClaimType = entity.ClaimType;
            ClaimValue = entity.ClaimValue;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
        }
    }


    public class DomainRoleClaimJsonConverter : JsonConverter<DomainRoleClaim> {

        public override DomainRoleClaim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainRoleClaim>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainRoleClaim value, JsonSerializerOptions options) {
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