using EDennis.AspNet.Base.Security.AspNetIdentityServer.Models.JsonConverters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserJsonConverter : JsonConverter<DomainUser> {

        private OtherProperties _otherProperties;

        public override DomainUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var obj = new DomainUser();
            DeserializeInto(obj, reader);
            return obj;
        }

        /// <summary>
        /// For Patch operations.  Do not include a [FromBody] parameter
        /// First, retrieve obj from store
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="request"></param>
        public void DeserializeInto(DomainUser obj, HttpRequest request) {
            using var reader = new StreamReader(request.Body, Encoding.UTF8);
            var str = Encoding.UTF8.GetBytes(Task.Run(() => reader.ReadToEndAsync()).Result);
            var bytes = new ReadOnlySpan<byte>(str);
            DeserializeInto(obj, new Utf8JsonReader(bytes));
        }

        public void DeserializeInto(DomainUser obj, Utf8JsonReader reader) {
            while (reader.Read()) {
                switch (reader.TokenType) {
                    case JsonTokenType.PropertyName:
                        var prop = reader.GetString();
                        reader.Read();
                        switch (prop) {
                            case "AccessFailedCount":
                            case "accessFailedCount":
                                obj.AccessFailedCount = reader.GetInt32();
                                break;
                            case "ConcurrencyStamp":
                            case "concurrencyStamp":
                                obj.ConcurrencyStamp = reader.GetString();
                                break;
                            case "Email":
                            case "email":
                                obj.Email = reader.GetString();
                                obj.NormalizedEmail = obj.Email.ToUpper();
                                break;
                            case "EmailConfirmed":
                            case "emailConfirmed":
                                obj.EmailConfirmed = reader.GetBoolean();
                                break;
                            case "Id":
                            case "id":
                                obj.Id = reader.GetGuid();
                                break;
                            case "LockoutBegin":
                            case "lockoutBegin":
                                obj.LockoutBegin = reader.GetDateTime();
                                break;
                            case "LockoutEnabled":
                            case "lockoutEnabled":
                                obj.LockoutEnabled = reader.GetBoolean();
                                break;
                            case "LockoutEnd":
                            case "lockoutEnd":
                                obj.LockoutEnd = reader.GetDateTime();
                                break;
                            case "NormalizedEmail":
                            case "normalizedEmail":
                                obj.NormalizedEmail = reader.GetString();
                                break;
                            case "NormalizedUserName":
                            case "normalizedUserName":
                                obj.NormalizedUserName = reader.GetString();
                                break;
                            case "OrganizationId":
                            case "organizationId":
                                obj.OrganizationId = reader.GetGuid();
                                break;
                            case "PasswordHash":
                            case "passwordHash":
                                obj.PasswordHash = reader.GetString();
                                break;
                            case "PhoneNumber":
                            case "phoneNumber":
                                obj.PhoneNumber = reader.GetString();
                                break;
                            case "PhoneNumberConfirmed":
                            case "phoneNumberConfirmed":
                                obj.PhoneNumberConfirmed = reader.GetBoolean();
                                break;
                            case "SecurityStamp":
                            case "securityStamp":
                                obj.SecurityStamp = reader.GetString();
                                break;
                            case "SysUser":
                            case "sysUser":
                                obj.SysUser = reader.GetString();
                                break;
                            case "SysStatus":
                            case "sysStatus":
                                obj.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), reader.GetString());
                                break;
                            case "SysStart":
                            case "sysStart":
                                obj.SysStart = reader.GetDateTime();
                                break;
                            case "SysEnd":
                            case "sysEnd":
                                obj.SysEnd = reader.GetDateTime();
                                break;
                            case "TwoFactorEnabled":
                            case "twoFactorEnabled":
                                obj.TwoFactorEnabled = reader.GetBoolean();
                                break;
                            case "UserName":
                            case "userName":
                                obj.UserName = reader.GetString();
                                obj.NormalizedUserName = obj.UserName.ToUpper();
                                break;
                            case "UserClaims__Packed":
                            case "userClaims__Packed":
                                Read_UserClaimsPacked(obj, ref reader);
                                break;
                            case "UserClaims":
                            case "userClaims":
                                Read_UserClaims(obj, ref reader);
                                break;
                            case "UserRoles":
                            case "userRoles":
                                Read_UserRoles(obj, ref reader);
                                break;
                            case "UserRoles__Packed":
                            case "userRoles__Packed":
                                Read_UserRolesPacked(obj, ref reader);
                                break;
                            case "UserLogins":
                            case "userLogins":
                                Read_UserLogins(obj, ref reader);
                                break;
                            case "UserTokens":
                            case "userTokens":
                                Read_UserTokens(obj, ref reader);
                                break;
                            default:
                                if (_otherProperties == null)
                                    _otherProperties = new OtherProperties();
                                _otherProperties.Add(prop, ref reader);
                                break;

                        }
                        break;
                    default:
                        break;
                }
            }
            if (_otherProperties != null) {
                obj.Properties = _otherProperties.ToString();
            }

        }

        #region UserRoles

        private void Read_UserRoles(DomainUser obj, ref Utf8JsonReader reader) {
            var currentDepth = reader.CurrentDepth;
            obj.UserRoles = new List<DomainUserRole>();
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndArray)) {
                Read_UserRole(obj.UserRoles, ref reader);
            }
        }

        private void Read_UserRole(ICollection<DomainUserRole> userRoles, ref Utf8JsonReader reader) {
            var userRole = new DomainUserRole();
            var currentDepth = reader.CurrentDepth;
            OtherProperties otherProperties = null;
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndObject)) {
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    string prop = reader.GetString();
                    reader.Read();
                    switch (prop) {
                        case "UserId":
                        case "userId":
                            userRole.UserId = reader.GetGuid();
                            break;
                        case "RoleId":
                        case "roleId":
                            userRole.RoleId = reader.GetGuid();
                            break;
                        case "SysUser":
                        case "sysUser":
                            userRole.SysUser = reader.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            userRole.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), reader.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            userRole.SysStart = reader.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            userRole.SysEnd = reader.GetDateTime();
                            break;
                        case "Role":
                        case "role":
                        case "User":
                        case "user":
                            break;
                        default:
                            if (otherProperties == null)
                                otherProperties = new OtherProperties();
                            otherProperties.Add(prop, ref reader);
                            break;
                    }
                }

            }
            if (otherProperties != null)
                userRole.Properties = otherProperties.ToString();
            userRoles.Add(userRole);
        }

        #endregion
        #region UserClaims

        private void Read_UserClaims(DomainUser obj, ref Utf8JsonReader reader) {
            var currentDepth = reader.CurrentDepth;
            obj.UserClaims = new List<DomainUserClaim>();
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndArray)) {
                Read_UserClaim(obj.UserClaims, ref reader);
            }
        }

        private void Read_UserClaim(ICollection<DomainUserClaim> userClaims, ref Utf8JsonReader reader) {
            var userClaim = new DomainUserClaim();
            var currentDepth = reader.CurrentDepth;
            OtherProperties otherProperties = null;
            string prop;
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndObject)) {
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    prop = reader.GetString();
                    reader.Read();
                    switch (prop) {
                        case "Id":
                        case "id":
                            userClaim.Id = reader.GetInt32();
                            break;
                        case "UserId":
                        case "userId":
                            userClaim.UserId = reader.GetGuid();
                            break;
                        case "ClaimType":
                        case "claimType":
                            userClaim.ClaimType = reader.GetString();
                            break;
                        case "ClaimValue":
                        case "claimValue":
                            userClaim.ClaimValue = reader.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            userClaim.SysUser = reader.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            userClaim.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), reader.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            userClaim.SysStart = reader.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            userClaim.SysEnd = reader.GetDateTime();
                            break;
                        case "User":
                        case "user":
                            break;
                        default:
                            if (otherProperties == null)
                                otherProperties = new OtherProperties();
                            otherProperties.Add(prop, ref reader);
                            break;
                    }
                }

            }
            if (otherProperties != null)
                userClaim.Properties = otherProperties.ToString();

            userClaims.Add(userClaim);
        }

        #endregion
        #region UserLogins

        private void Read_UserLogins(DomainUser obj, ref Utf8JsonReader reader) {
            obj.UserLogins = new List<DomainUserLogin>();
            var currentDepth = reader.CurrentDepth;
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndArray)) {
                Read_UserLogin(obj.UserLogins, ref reader);
            }
        }

        private void Read_UserLogin(ICollection<DomainUserLogin> userLogins, ref Utf8JsonReader reader) {
            var userLogin = new DomainUserLogin();
            var currentDepth = reader.CurrentDepth;
            string prop;
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndObject)) {
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    prop = reader.GetString();
                    reader.Read();
                    switch (prop) {
                        case "UserId":
                        case "userId":
                            userLogin.UserId = reader.GetGuid();
                            break;
                        case "LoginProvider":
                        case "loginProvider":
                            userLogin.LoginProvider = reader.GetString();
                            break;
                        case "ProviderKey":
                        case "providerKey":
                            userLogin.ProviderKey = reader.GetString();
                            break;
                        case "ProviderDisplayName":
                        case "providerDisplayName":
                            userLogin.ProviderDisplayName = reader.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            userLogin.SysUser = reader.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            userLogin.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), reader.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            userLogin.SysStart = reader.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            userLogin.SysEnd = reader.GetDateTime();
                            break;
                        case "User":
                        case "user":
                            break;
                        default:
                            break;
                    }
                }

            }
            userLogins.Add(userLogin);
        }

        #endregion
        #region UserTokens
        private void Read_UserTokens(DomainUser obj, ref Utf8JsonReader reader) {
            obj.UserTokens = new List<DomainUserToken>();
            var currentDepth = reader.CurrentDepth;
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndArray)) {
                Read_UserToken(obj.UserTokens, ref reader);
            }
        }

        private void Read_UserToken(ICollection<DomainUserToken> userTokens, ref Utf8JsonReader reader) {
            var userToken = new DomainUserToken();
            var currentDepth = reader.CurrentDepth;
            string prop;
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndObject)) {
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    prop = reader.GetString();
                    reader.Read();
                    switch (prop) {
                        case "UserId":
                        case "userId":
                            userToken.UserId = reader.GetGuid();
                            break;
                        case "LoginProvider":
                        case "loginProvider":
                            userToken.LoginProvider = reader.GetString();
                            break;
                        case "Name":
                        case "name":
                            userToken.Name = reader.GetString();
                            break;
                        case "Value":
                        case "value":
                            userToken.Value = reader.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            userToken.SysUser = reader.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            userToken.SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), reader.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            userToken.SysStart = reader.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            userToken.SysEnd = reader.GetDateTime();
                            break;
                        case "User":
                        case "user":
                            break;
                        default:
                            break;
                    }
                }

            }
            userTokens.Add(userToken);
        }

        #endregion
        #region UserClaimsPacked

        private void Read_UserClaimsPacked(DomainUser obj, ref Utf8JsonReader reader) {
            var currentDepth = reader.CurrentDepth;
            obj.UserClaims__Packed = new Dictionary<string, List<string>>();

            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndObject)) {
                string claimType;
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    claimType = reader.GetString();
                    var claimValues = new List<string>();
                    Read_UserClaimPacked(claimValues, ref reader);
                    obj.UserClaims__Packed.Add(claimType, claimValues);
                }
            }
        }

        private void Read_UserClaimPacked(List<string> claimValues, ref Utf8JsonReader reader) {
            var currentDepth = reader.CurrentDepth;
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndArray)) {
                reader.Read();
                claimValues.Add(reader.GetString());
            }
        }

        #endregion
        #region UserRolesPacked

        private void Read_UserRolesPacked(DomainUser obj, ref Utf8JsonReader reader) {
            var currentDepth = reader.CurrentDepth;
            obj.UserRoles__Packed = new Dictionary<string, List<string>>();

            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndObject)) {
                string appName = "";
                reader.Read();
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    appName = reader.GetString();
                    var roleNames = new List<string>();
                    Read_UserRolePacked(roleNames, ref reader);
                    obj.UserRoles__Packed.Add(appName, roleNames);
                }
            }
        }

        private void Read_UserRolePacked(List<string> roleNames, ref Utf8JsonReader reader) {
            var currentDepth = reader.CurrentDepth;
            while (!(reader.CurrentDepth == currentDepth && reader.TokenType == JsonTokenType.EndArray)) {
                reader.Read();
                roleNames.Add(reader.GetString());
            }
        }

        #endregion

        public override void Write(Utf8JsonWriter writer, DomainUser value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("UserName", value.UserName);
                if (value.AccessFailedCount != default)
                    writer.WriteNumber("AccessFailedCount", value.AccessFailedCount);
                writer.WriteString("Email", value.Email);
                writer.WriteBoolean("EmailConfirmed", value.EmailConfirmed);
                writer.WriteBoolean("LockoutEnabled", value.LockoutEnabled);
                if (value.LockoutBegin != default)
                    writer.WriteString("LockoutBegin", value.LockoutBegin.Value.ToString("u"));
                if (value.LockoutEnd != default)
                    writer.WriteString("LockoutEnd", value.LockoutEnd.Value.ToString("u"));
                if (value.OrganizationId != default)
                    writer.WriteString("OrganizationId", value.OrganizationId.ToString());
                if (value.Organization != null)
                    writer.WriteString("OrganizationName", value.Organization.Name);
                if (value.PhoneNumber != default)
                    writer.WriteString("PhoneNumber", value.PhoneNumber);
                if (value.PhoneNumberConfirmed != default)
                    writer.WriteBoolean("PhoneNumberConfirmed", value.PhoneNumberConfirmed);
                if (value.TwoFactorEnabled != default)
                    writer.WriteBoolean("TwoFactorEnabled", value.TwoFactorEnabled);
                if (value.UserClaims != null && value.UserClaims.Count > 0) {
                    writer.WriteStartArray("Claims");
                    {
                        foreach (var entry in value.UserClaims) {
                            writer.WriteStartObject();
                            {
                                writer.WriteNumber("Id",entry.Id);
                                writer.WriteString("ClaimType", entry.ClaimType);
                                writer.WriteString("ClaimValue", entry.ClaimValue);
                                writer.WriteString("SysUser", entry.SysUser);
                                writer.WriteString("SysStatus", entry.SysStatus.ToString());
                                writer.WriteString("SysStart", entry.SysStart.ToString("u"));
                                writer.WriteString("SysEnd", entry.SysEnd.ToString("u"));
                            }
                            writer.WriteEndObject();
                        }
                    }
                    writer.WriteEndArray();
                }
                if (value.UserRoles != null && value.UserRoles.Count > 0) {
                    writer.WriteStartArray("Roles");
                    {
                        foreach (var entry in value.UserRoles) {
                            writer.WriteStartObject();
                            {
                                writer.WriteString("UserId", entry.UserId);
                                writer.WriteString("RoleId", entry.RoleId);
                                if (entry.Role != null) {
                                    writer.WriteString("RoleName", entry.Role.Name);
                                    if(entry.Role.Application != null)
                                        writer.WriteString("ApplicationName", entry.Role.Application.Name);
                                }
                                writer.WriteString("SysUser", entry.SysUser);
                                writer.WriteString("SysStatus", entry.SysStatus.ToString());
                                writer.WriteString("SysStart", entry.SysStart.ToString("u"));
                                writer.WriteString("SysEnd", entry.SysEnd.ToString("u"));
                            }
                            writer.WriteEndObject();
                        }
                    }
                    writer.WriteEndArray();
                }

                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
                //extract catch-all properties and promote to top-level in JSON
                if (value.Properties != null) {
                    using var doc = JsonDocument.Parse(value.Properties);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        prop.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
        }

    }

}
