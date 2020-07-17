using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainRoleClaimJsonConverter))]

    public class DomainRoleClaim : IdentityRoleClaim<Guid>, ITemporalEntity {

        public DomainRole Role { get; set; }


        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true)
            => DeserializeInto(jsonElement, this, modelState);


        public void Update(object updated) {
            var obj = updated as DomainRoleClaim;
            Id = obj.Id;
            RoleId = obj.RoleId;
            ClaimType = obj.ClaimType;
            ClaimValue = obj.ClaimValue;
            SysUser = obj.SysUser;
            SysStatus = obj.SysStatus;
            SysStart = obj.SysStart;
            SysEnd = obj.SysEnd;
        }

        public static DomainRoleClaim DeserializeInto(JsonElement jsonElement, DomainRoleClaim claim, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Id":
                        case "id":
                            claim.Id = prop.Value.GetInt32();
                            break;
                        case "RoleId":
                        case "userId":
                            claim.RoleId = prop.Value.GetGuid();
                            break;
                        case "Name":
                        case "name":
                            claim.ClaimType = prop.Value.GetString();
                            break;
                        case "Value":
                        case "value":
                            claim.ClaimValue = prop.Value.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            claim.SysUser = prop.Value.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            claim.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            claim.SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            claim.SysEnd = prop.Value.GetDateTime();
                            break;
                        default:
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainRoleClaim).Name} JSON");
                }

            }
            return claim;

        }

    }

    /// <summary>
    /// Explicitly define JsonConverter to prevent circular referencing during Serialization
    /// </summary>
    public class DomainRoleClaimJsonConverter : JsonConverter<DomainRoleClaim> {

        public override DomainRoleClaim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainRoleClaim>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainRoleClaim value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteNumber("Id", value.Id);
                writer.WriteString("RoleId", value.RoleId.ToString());
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

