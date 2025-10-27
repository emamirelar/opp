using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models.AI
{
    public class AiChatRequest
    {
        [JsonPropertyName("app_name")]
        public required string AppName { get; set; }

        [JsonPropertyName("user_id")]
        public required string UserId { get; set; }

        [JsonPropertyName("user_email")]
        public required string UserEmail { get; set; }

        [JsonPropertyName("session_id")]
        public required string SessionId { get; set; }

        [JsonPropertyName("message")]
        public required string Message { get; set; }

        [JsonPropertyName("streaming")]
        public bool Streaming { get; set; } = false;

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }


} 