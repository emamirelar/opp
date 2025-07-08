using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models
{
    public class AiChatRequest
    {
        [JsonPropertyName("app_name")]
        public string AppName { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("session_id")]
        public string SessionId { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("streaming")]
        public bool Streaming { get; set; } = false;

        [JsonPropertyName("state")]
        public Dictionary<string, object>? State { get; set; }
    }


} 