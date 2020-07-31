using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace EDennis.NetStandard.Base {
    
    /// <summary>
    /// This class provides some convenience methods
    /// for deserializing JSON content returned from
    /// an HttpClient's response message
    /// </summary>
    /// <typeparam name="T">The target type for deserialization</typeparam>
    public class BodyContent<T> : StringContent {

        /// <summary>
        /// Constructs a new BodyContext object
        /// with the default encoding and media type
        /// </summary>
        /// <param name="obj">the object to deserialize</param>
        public BodyContent(T obj) :
            base(JsonSerializer.Serialize(obj).ToString(),
                Encoding.UTF8,
                "application/json") { }

        /// <summary>
        /// Constructs a new BodyContext object
        /// with the specified encoding and the
        /// default media type
        /// </summary>
        /// <param name="obj">object to deserialize</param>
        /// <param name="encoding">string encoding type</param>
        public BodyContent(T obj, Encoding encoding) :
            base(JsonSerializer.Serialize(obj).ToString(), encoding) {
        }

        /// <summary>
        /// Constructs a new BodyContent object
        /// with the specified encoding and media type
        /// </summary>
        /// <param name="obj">object to deserialize</param>
        /// <param name="encoding">string encoding type</param>
        /// <param name="mediaType">media type</param>
        public BodyContent(T obj, Encoding encoding, string mediaType) :
            base(JsonSerializer.Serialize(obj).ToString(), encoding, mediaType) {
        }
    }
}
