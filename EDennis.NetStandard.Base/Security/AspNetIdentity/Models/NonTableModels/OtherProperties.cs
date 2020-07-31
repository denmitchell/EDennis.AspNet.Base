using System.Collections.Generic;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    public class OtherProperties {
        private readonly List<string> props = new List<string>();

        public override string ToString() {
            return "{" + string.Join(",", props) +"}";
        }

        public void Add(string propertyName, ref Utf8JsonReader reader) {
            switch (reader.TokenType) {
                case JsonTokenType.Null:
                    props.Add($"{propertyName}:null");
                    break;
                case JsonTokenType.True:
                    props.Add($"{propertyName}:true");
                    break;
                case JsonTokenType.False:
                    props.Add($"{propertyName}:false");
                    break;
                case JsonTokenType.Number:
                    props.Add($"{propertyName}:{reader.GetDecimal()}");
                    break;
                case JsonTokenType.String:
                    props.Add($"{propertyName}:\"{reader.GetString()}\"");
                    break;
                default:
                    break;
            }
        }


        public void Add(JsonProperty prop) {
            switch (prop.Value.ValueKind) {
                case JsonValueKind.Null:
                    props.Add($"{prop.Name}:null");
                    break;
                case JsonValueKind.True:
                    props.Add($"{prop.Name}:true");
                    break;
                case JsonValueKind.False:
                    props.Add($"{prop.Name}:false");
                    break;
                case JsonValueKind.Number:
                    props.Add($"{prop.Name}:{prop.Value.GetDecimal()}");
                    break;
                case JsonValueKind.String:
                    props.Add($"{prop.Name}:\"{prop.Value.GetString()}\"");
                    break;
                default:
                    break;
            }
        }

    }
}
