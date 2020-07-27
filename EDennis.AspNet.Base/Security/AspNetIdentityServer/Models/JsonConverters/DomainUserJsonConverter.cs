using Microsoft.AspNetCore.Http;
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
        public static void DeserializeInto(DomainUser obj, HttpRequest request) {
            using var reader = new StreamReader(request.Body, Encoding.UTF8);
            var str = Encoding.UTF8.GetBytes(Task.Run(() => reader.ReadToEndAsync()).Result);
            var bytes = new ReadOnlySpan<byte>(str);
            DeserializeInto(obj, new Utf8JsonReader(bytes));
        }

        public static void DeserializeInto(DomainUser obj, Utf8JsonReader reader) {
            bool hasWrittenProperties = false;
            using var ms = new MemoryStream();
            using var jw = new Utf8JsonWriter(ms);
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
                                var unpackedClaims = new List<DomainUserClaim>();
                                foreach (var packedClaim in reader.EnumerateObject())
                                    foreach (var packedValue in packedClaim.Value.EnumerateArray())
                                        unpackedClaims.Add(new DomainUserClaim { ClaimType = packedClaim.Name, ClaimValue = packedValue.GetString() });
                                obj.UserClaims = unpackedClaims.ToArray();
                                break;
                            case "UserClaims":
                            case "userClaims":
                                var claims = new List<DomainUserClaim>();
                                foreach (var item in reader.EnumerateArray()) {
                                    var claim = new DomainUserClaim();
                                    claim.DeserializeInto(item, modelState);
                                    claims.Add(claim);
                                }
                                obj.UserClaims = claims.ToArray();
                                break;
                            case "UserRoles":
                            case "userRoles":
                                obj.UserRoles = new List<DomainUserRole>();
                                var urDepth = reader.CurrentDepth;
                                while (reader.Read()) {
                                    if (reader.CurrentDepth > urDepth + 2)
                                        continue;
                                    else if (reader.TokenType == JsonTokenType.EndArray)
                                        break;
                                    else if (reader.TokenType == JsonTokenType.StartObject) {
                                        obj.UserRoles.Add(new DomainUserRole { UserId = obj.Id });
                                    } else if (reader.TokenType == JsonTokenType.PropertyName) {
                                        var prop2 = reader.GetString();
                                        reader.Read();
                                        if (prop2 == "RoleId" || prop2 == "roleId")
                                            obj.UserRoles.Last().RoleId = reader.GetGuid();
                                    }
                                }
                                break;
                            case "UserRoles__Packed":
                            case "userRoles__Packed":
                                obj.UserRoles__Packed = new Dictionary<string, string[]>();
                                while (reader.Read()) {
                                    if (reader.TokenType == JsonTokenType.EndObject)
                                        break;
                                    else if (reader.TokenType == JsonTokenType.PropertyName) {
                                        var prop2 = reader.GetString();
                                        var roleNames = new List<string>();
                                        while (reader.Read()) {
                                            if (reader.TokenType == JsonTokenType.StartArray)
                                        }
                                        if (prop2 == "RoleId" || prop2 == "roleId")
                                            obj.UserRoles.Last().RoleId = reader.GetGuid();
                                    } else if (reader.TokenType == JsonTokenType.StartObject) {
                                        obj.UserRoles.Add(new DomainUserRole { UserId = obj.Id });
                                    }
                                }
                                //var roleNames = reader.EnumerateArray().Select(x => x.GetString());
                                //obj.UserRoles = dbContext.Set<DomainRole>()
                                //    .Where(r => roleNames.Any(n => n == r.Name))
                                //    .Select(r => new DomainUserRole { UserId = Id, RoleId = r.Id })
                                //    .ToArray();
                                break;
                            case "UserLogins":
                            case "userLogins":
                                obj.UserLogins = new List<DomainUserLogin>();
                                var ulDepth = reader.CurrentDepth;
                                while (reader.Read()) {
                                    if (reader.CurrentDepth > ulDepth + 2)
                                        continue;
                                    else if (reader.TokenType == JsonTokenType.EndArray)
                                        break;
                                    else if (reader.TokenType == JsonTokenType.StartObject) {
                                        obj.UserLogins.Add(new DomainUserLogin { UserId = obj.Id });
                                    } else if (reader.TokenType == JsonTokenType.PropertyName) {
                                        var prop2 = reader.GetString();
                                        reader.Read();
                                        if (prop2 == "LoginProvider" || prop2 == "loginProvider")
                                            obj.UserLogins.Last().LoginProvider = reader.GetString();
                                        else if (prop2 == "ProviderKey" || prop2 == "providerKey")
                                            obj.UserLogins.Last().ProviderKey = reader.GetString();
                                        else if (prop2 == "ProviderDisplayName" || prop2 == "providerDisplayName")
                                            obj.UserLogins.Last().ProviderDisplayName = reader.GetString();
                                    }
                                }
                                break;
                            case "UserTokens":
                            case "userTokens":
                                obj.UserTokens = new List<DomainUserToken>();
                                var utDepth = reader.CurrentDepth;
                                while (reader.Read()) {
                                    if (reader.CurrentDepth > utDepth + 2)
                                        continue;
                                    else if (reader.TokenType == JsonTokenType.EndArray)
                                        break;
                                    else if (reader.TokenType == JsonTokenType.StartObject) {
                                        obj.UserTokens.Add(new DomainUserToken { UserId = obj.Id });
                                    } else if (reader.TokenType == JsonTokenType.PropertyName) {
                                        var prop2 = reader.GetString();
                                        reader.Read();
                                        if (prop2 == "LoginProvider" || prop2 == "loginProvider")
                                            obj.UserTokens.Last().LoginProvider = reader.GetString();
                                        else if (prop2 == "Value" || prop2 == "value")
                                            obj.UserTokens.Last().Value = reader.GetString();
                                    }
                                }
                                break;
                            default:
                                break;

                        }
                        break;
                    default:
                        break;
                }
            }
            if (hasWrittenProperties) {
                jw.WriteEndObject();
                obj.Properties = Encoding.UTF8.GetString(ms.ToArray());
            }

        }


        private void Read_OtherProperties(DomainUser obj, ref Utf8JsonReader reader, bool hasWrittenProperties) {
            if (!hasWrittenProperties)
                jw.WriteStartObject();
            switch (reader.TokenType) {
                case JsonTokenType.Null:
                    jw.WriteNull(prop);
                    break;
                case JsonTokenType.True:
                case JsonTokenType.False:
                    jw.WriteBoolean(prop, reader.GetBoolean());
                    break;
                case JsonTokenType.Number:
                    jw.WriteNumber(prop, reader.GetDecimal());
                    break;
                case JsonTokenType.String:
                    jw.WriteString(prop, reader.GetString());
                    break;
                default:
                    break;
            }
        }



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
                    var dict = value.UserClaims.ToDictionary();
                    writer.WriteStartObject("Claims");
                    {
                        foreach (var entry in dict) {
                            writer.WritePropertyName(entry.Key);
                            writer.WriteStartArray();
                            {
                                foreach (var item in entry.Value)
                                    writer.WriteStringValue(item);
                            }
                            writer.WriteEndArray();
                        }
                    }
                    writer.WriteEndObject();
                }
                if (value.UserRoles != null && value.UserRoles.Count > 0) {
                    var roles = value.UserRoles.Select(r => r.Role.Name);
                    writer.WriteStartArray("Roles");
                    {
                        foreach (var role in roles)
                            writer.WriteStringValue(role);
                    }
                    writer.WriteEndObject();
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
