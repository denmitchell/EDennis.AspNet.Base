using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainUserLoginJsonConverter))]
    public class DomainUserLogin : IdentityUserLogin<Guid>, ITemporalEntity {

        public DomainUser User { get; set; }


        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true)
            => DeserializeInto(jsonElement, this, modelState);


        public void Update(object updated) {
            var obj = updated as DomainUserLogin;
            UserId = obj.UserId;
            LoginProvider = obj.LoginProvider;
            ProviderKey = obj.ProviderKey;
            ProviderDisplayName = obj.ProviderDisplayName;
            SysUser = obj.SysUser;
            SysStatus = obj.SysStatus;
            SysStart = obj.SysStart;
            SysEnd = obj.SysEnd;
        }

        public static DomainUserLogin DeserializeInto(JsonElement jsonElement, DomainUserLogin login, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "UserId":
                        case "userId":
                            login.UserId = prop.Value.GetGuid();
                            break;
                        case "LoginProvider":
                        case "loginProvider":
                            login.LoginProvider = prop.Value.GetString();
                            break;
                        case "ProviderKey":
                        case "providerKey":
                            login.ProviderKey = prop.Value.GetString();
                            break;
                        case "ProviderDisplayName":
                        case "providerDisplayName":
                            login.ProviderDisplayName = prop.Value.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            login.SysUser = prop.Value.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            login.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            login.SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            login.SysEnd = prop.Value.GetDateTime();
                            break;
                        default:
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {typeof(DomainUserToken).Name} JSON");
                }

            }
            return login;

        }

    }

}
