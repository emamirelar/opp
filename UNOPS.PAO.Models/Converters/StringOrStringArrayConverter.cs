using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UNOPS.PAO.Models.Converters
{
    public class StringOrStringArrayConverter : JsonConverter<List<string>?>
    {
        public override List<string>? ReadJson(JsonReader reader, Type objectType, List<string>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            
            if (token.Type == JTokenType.Null)
            {
                return null;
            }
            
            if (token.Type == JTokenType.Array)
            {
                // Already an array - convert to List<string>
                return token.ToObject<List<string>>();
            }
            
            if (token.Type == JTokenType.String)
            {
                // Single string - convert to List<string> with one item
                var stringValue = token.ToString();
                return string.IsNullOrEmpty(stringValue) ? new List<string>() : new List<string> { stringValue };
            }
            
            // Fallback - try to convert whatever it is to string and wrap in list
            return new List<string> { token.ToString() };
        }

        public override void WriteJson(JsonWriter writer, List<string>? value, JsonSerializer serializer)
        {
            // Always serialize as array
            serializer.Serialize(writer, value);
        }
    }
}
