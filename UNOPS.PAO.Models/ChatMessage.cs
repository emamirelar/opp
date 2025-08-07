using Newtonsoft.Json;

namespace UNOPS.PAO.Models
{
    public class ChatMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }
        
        [JsonProperty("text")]
        public string Text { get; set; }
        
        [JsonProperty("timestamp")]
        public DateTime? Timestamp { get; set; }
        
        [JsonProperty("inlineData")]
        public List<InlineData> InlineData { get; set; } = new List<InlineData>();
    }

    public class InlineData
    {
        [JsonProperty("data")]
        public string Data { get; set; }
        
        [JsonProperty("mimeType")]
        public string MimeType { get; set; }
    }
} 