
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    [JsonConverter(typeof(DomainApplicationViewJsonConverter))]
    public class DomainApplicationView : DomainApplication {

        public string Roles { get; set; }

    }

    public class DomainApplicationViewJsonConverter : JsonConverter<DomainApplicationView> {

        public override DomainApplicationView Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            return (DomainApplicationView)JsonSerializer.Deserialize(ref reader, typeof(DomainApplicationView), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }


        public override void Write(Utf8JsonWriter writer, DomainApplicationView value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            {
                writer.WriteString("Name", value.Name);
                if (value.Roles != null) {
                    writer.WriteStartObject("Roles");
                    {
                        using var doc = JsonDocument.Parse(value.Roles);
                        foreach (var prop in doc.RootElement.EnumerateObject())
                            prop.WriteTo(writer);
                    }
                    writer.WriteEndObject();
                }
            }
            writer.WriteEndObject();
        }

    }

}
