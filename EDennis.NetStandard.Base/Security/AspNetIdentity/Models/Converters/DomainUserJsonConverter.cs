using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Used to handle serialization/deserialization of Properties property
    /// </summary>
    public class DomainUserJsonConverter : JsonConverter<DomainUser> {

        private OtherProperties _otherProperties;

        public override DomainUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            var obj = new DomainUser();
            DeserializeInto(obj, reader);
            return obj;
        }

        /// <summary>
        /// This method can be used to directly deserialize an HttpRequest body
        /// into an object.
        /// If used, do not include a [FromBody] parameter
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


        public override void Write(Utf8JsonWriter writer, DomainUser value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("UserName", value.UserName);
                writer.WriteString("NormalizedUserName", value.NormalizedUserName);
                if (value.AccessFailedCount != default)
                    writer.WriteNumber("AccessFailedCount", value.AccessFailedCount);
                writer.WriteString("Email", value.Email);
                writer.WriteString("NormalizedEmail", value.NormalizedEmail);
                writer.WriteBoolean("EmailConfirmed", value.EmailConfirmed);
                writer.WriteBoolean("LockoutEnabled", value.LockoutEnabled);
                if (value.LockoutBegin != default)
                    writer.WriteString("LockoutBegin", value.LockoutBegin.Value.ToString("u"));
                if (value.LockoutEnd != default)
                    writer.WriteString("LockoutEnd", value.LockoutEnd.Value.ToString("u"));
                if (value.OrganizationId != default)
                    writer.WriteString("OrganizationId", value.OrganizationId.ToString());
                if (value.PhoneNumber != default)
                    writer.WriteString("PhoneNumber", value.PhoneNumber);
                if (value.PhoneNumberConfirmed != default)
                    writer.WriteBoolean("PhoneNumberConfirmed", value.PhoneNumberConfirmed);
                if (value.TwoFactorEnabled != default)
                    writer.WriteBoolean("TwoFactorEnabled", value.TwoFactorEnabled);
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
