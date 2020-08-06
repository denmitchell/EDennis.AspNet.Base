using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    /// <summary>
    /// Used to handle serialization/deserialization of Properties property
    /// </summary>
    public class ExpandedDomainUserJsonConverter : JsonConverter<ExpandedDomainUser> {

        private OtherProperties _otherProperties;

        public override ExpandedDomainUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            return (ExpandedDomainUser)JsonSerializer.Deserialize(ref reader, typeof(ExpandedDomainUser), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }


        public override void Write(Utf8JsonWriter writer, ExpandedDomainUser value, JsonSerializerOptions options) {
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
                if (value.RolesDictionary != null) {
                    using var doc = JsonDocument.Parse(value.RolesDictionary);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        prop.WriteTo(writer);
                }
                if (value.ClaimsDictionary != null) {
                    using var doc = JsonDocument.Parse(value.ClaimsDictionary);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        prop.WriteTo(writer);
                }
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
