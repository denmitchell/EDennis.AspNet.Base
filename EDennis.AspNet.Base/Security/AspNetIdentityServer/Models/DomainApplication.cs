using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.AspNet.Base.Security {

    [JsonConverter(typeof(DomainApplicationJsonConverter))]
    public class DomainApplication : ITemporalEntity {
        public Guid Id { get; set; } = CombGuid.Create();
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public SysStatus SysStatus { get; set; }
        public string SysUser { get; set; }
        public DateTime SysStart { get; set; }
        public DateTime SysEnd { get; set; }

        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState, bool mergeCollections = true) {
            foreach (var prop in jsonElement.EnumerateObject()) {
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
                        case "Properties":
                        case "properties":
                            var properties = new Dictionary<string, string>();
                            prop.Value.EnumerateObject().ToList().ForEach(e => {
                                properties.Add(e.Name, e.Value.GetString());
                            });
                            if (mergeCollections && Properties != null)
                                foreach (var entry in properties)
                                    if (Properties.ContainsKey(entry.Key))
                                        Properties[entry.Key] = entry.Value;
                                    else
                                        Properties.Add(entry.Key, entry.Value);
                            else
                                Properties = properties;
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
                            break;
                    }
                } catch (InvalidOperationException ex) {
                    modelState.AddModelError(prop.Name, $"{ex.Message}: Cannot parse value for {prop.Value} from {GetType().Name} JSON");
                }
            }
        }

        public void Update(object updated) {
            var obj = updated as DomainApplication;
            Id = obj.Id;
            Name = obj.Name;
            Properties = obj.Properties;
            SysStart = obj.SysStart;
            SysEnd = obj.SysEnd;
            SysStatus = obj.SysStatus;
            SysUser = obj.SysUser;
        }

    }

    /// <summary>
    /// Explicitly define JsonConverter to prevent circular referencing during Serialization
    /// </summary>
    public class DomainApplicationJsonConverter : JsonConverter<DomainApplication> {

        public override DomainApplication Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => JsonSerializer.Deserialize<DomainApplication>(ref reader, options);


        public override void Write(Utf8JsonWriter writer, DomainApplication value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Id", value.Id.ToString());
                writer.WriteString("Name", value.Name);
                writer.WriteString("SysUser", value.SysUser);
                writer.WriteString("SysStatus", value.SysStatus.ToString());
                writer.WriteString("SysStart", value.SysStart.ToString("u"));
                writer.WriteString("SysEnd", value.SysStart.ToString("u"));
                if (value.Properties != null && value.Properties.Count > 0) {
                    writer.WriteStartObject("Properties");
                    {
                        foreach (var prop in value.Properties) {
                            writer.WriteString(prop.Key, prop.Value);
                        }
                    }
                    writer.WriteEndArray();
                }
            }
            writer.WriteEndObject();
        }
    }
}
