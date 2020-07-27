using System.Collections.Generic;
using System.Text.Json;

namespace EDennis.AspNet.Base.Security.AspNetIdentityServer.Models.JsonConverters {
    public class OtherProperties {
        private readonly List<string> props = new List<string>();

        public override string ToString() {
            return "{" + string.Join(',', props) +"}";
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
    }
}
