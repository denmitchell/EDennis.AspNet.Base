
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EDennis.NetStandard.Base {

    [JsonConverter(typeof(DomainApplicationViewJsonConverter))]
    public class DomainApplicationView : ICrudEntity {

        public string Name { get; set; }

        public string SysUser { get; set; }
        public SysStatus SysStatus { get; set; }

        public string Roles { get; set; }


        public void Patch(JsonElement jsonElement, ModelStateDictionary modelState) {
            foreach (var prop in jsonElement.EnumerateObject()) {
                switch (prop.Name) {
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
                }
            }
        }

        public void Update(object updated) {
            var entity = updated as DomainApplication;
            Name = entity.Name;
            SysUser = entity.SysUser;
            SysStatus = entity.SysStatus;
        }


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
