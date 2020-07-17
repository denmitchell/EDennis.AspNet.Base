using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {


    [JsonConverter(typeof(DomainUserClaimJsonConverter))]
    public class DomainUserClaim : IdentityUserClaim<Guid>, ITemporalEntity {

        public DomainUser User { get; set; }


        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true)
            => DeserializeInto(jsonElement, this, modelState);


        public void Update(object updated) {
            var obj = updated as DomainUserClaim;
            Id = obj.Id;
            UserId = obj.UserId;
            ClaimType = obj.ClaimType;
            ClaimValue = obj.ClaimValue;
            SysUser = obj.SysUser;
            SysStatus = obj.SysStatus;
            SysStart = obj.SysStart;
            SysEnd = obj.SysEnd;
        }

        public static DomainUserClaim DeserializeInto(JsonElement jsonElement, DomainUserClaim claim, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Id":
                        case "id":
                            claim.Id = prop.Value.GetInt32();
                            break;
                        case "UserId":
                        case "userId":
                            claim.UserId = prop.Value.GetGuid();
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
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainUserClaim).Name} JSON");
                }

            }
            return claim;

        }

    }

    /// <summary>
    /// Explicitly define JsonConverter to prevent circular referencing during Serialization
    /// </summary>
    public class DomainUserClaimJsonConverter : JsonConverter<DomainUserClaim> {

        public override DomainUserClaim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUserClaim>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainUserClaim value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteNumber("Id", value.Id);
                writer.WriteString("UserId", value.UserId.ToString());
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
