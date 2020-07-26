using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainUserTokenJsonConverter))]
    public class DomainUserToken : IdentityUserToken<Guid>, ITemporalEntity {

        public DomainUser User { get; set; }


        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true)
            => DeserializeInto(jsonElement, this, modelState);


        public void Update(object updated) {
            var obj = updated as DomainUserToken;
            UserId = obj.UserId;
            LoginProvider = obj.LoginProvider;
            Name = obj.Name;
            Value = obj.Value;
            SysUser = obj.SysUser;
            SysStatus = obj.SysStatus;
            SysStart = obj.SysStart;
            SysEnd = obj.SysEnd;
        }

        public static DomainUserToken DeserializeInto(JsonElement jsonElement, DomainUserToken token, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "UserId":
                        case "userId":
                            token.UserId = prop.Value.GetGuid();
                            break;
                        case "LoginProvider":
                        case "loginProvider":
                            token.LoginProvider = prop.Value.GetString();
                            break;
                        case "Name":
                        case "name":
                            token.Name = prop.Value.GetString();
                            break;
                        case "Value":
                        case "value":
                            token.Value = prop.Value.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            token.SysUser = prop.Value.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            token.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            token.SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            token.SysEnd = prop.Value.GetDateTime();
                            break;
                        default:
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainUserToken).Name} JSON");
                }

            }
            return token;

        }

    }

}
