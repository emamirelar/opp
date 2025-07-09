using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models
{
    public class SessionWithChats
    {
        public AiChatSession Session { get; set; }
        public List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
} 