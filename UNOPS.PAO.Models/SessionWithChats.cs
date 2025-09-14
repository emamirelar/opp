using UNOPS.PAO.Domain.Entities;
using Newtonsoft.Json;

namespace UNOPS.PAO.Models
{
    public class SessionWithChats
    {
        [JsonProperty("session")]
        public AiChatSession Session { get; set; }
        
        [JsonProperty("chatMessages")]
        public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
} 