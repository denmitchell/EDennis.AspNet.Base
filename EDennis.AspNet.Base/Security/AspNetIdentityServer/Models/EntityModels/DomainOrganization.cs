using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainOrganizationJsonConverter))]
    public class DomainOrganization : IDomainEntity, IHasStringProperties {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public ICollection<DomainUser> Users { get; set; }
        public string Properties { get; set; }
        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }


        public void DeserializeInto(JsonElement source, ModelStateDictionary modelState) {
            bool hasWrittenProperties = false;
            using var ms = new MemoryStream();
            using var jw = new Utf8JsonWriter(ms);
            foreach (var prop in source.EnumerateObject()) {
                try {
                    switch (prop.Name) {
                        case "Id":
                        case "id":
                            Id = prop.Value.GetGuid();
                            break;
                        case "Name":
                        case "name":
                            Name = prop.Value.GetString();
                            break;
                        case "SysUser":
                        case "sysUser":
                            SysUser = prop.Value.GetString();
                            break;
                        case "SysStatus":
                        case "sysStatus":
                            SysStatus = (SysStatus)Enum.Parse(typeof(SysStatus), prop.Value.GetString());
                            break;
                        case "SysStart":
                        case "sysStart":
                            SysStart = prop.Value.GetDateTime();
                            break;
                        case "SysEnd":
                        case "sysEnd":
                            SysEnd = prop.Value.GetDateTime();
                            break;
                        default:
                            if (!hasWrittenProperties)
                                jw.WriteStartObject();
                            prop.WriteTo(jw);
                            break;
                    }
                } catch (Exception ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {GetType().Name} JSON");
                }
                if (hasWrittenProperties) {
                    jw.WriteEndObject();
                    Properties = Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

    }

    /// <summary>
    /// Explicitly define JsonConverter to prevent circular referencing during Serialization
    /// </summary>
    public class DomainOrganizationJsonConverter : JsonConverter<DomainOrganization> {

        public override DomainOrganization Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainOrganization>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainOrganization value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("Name", value.Name);
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
