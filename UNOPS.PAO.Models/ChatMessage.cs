namespace UNOPS.PAO.Models
{
    public class ChatMessage
    {
        public string Role { get; set; }
        public string Text { get; set; }
        public DateTime? Timestamp { get; set; }
    }
} 