using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models
{
    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
        
        [JsonPropertyName("text")]
        public string Text { get; set; }
        
        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }
        
        [JsonPropertyName("inlineData")]
        public List<InlineData> InlineData { get; set; } = new List<InlineData>();
    }

    public class InlineData
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }
        
        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; }
    }
} 