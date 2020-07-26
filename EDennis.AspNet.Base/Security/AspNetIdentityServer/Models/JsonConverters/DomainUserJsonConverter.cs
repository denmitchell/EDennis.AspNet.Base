using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {
    public class DomainUserJsonConverter : JsonConverter<DomainUser> {
        public override DomainUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainUser>(ref reader, options);

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
