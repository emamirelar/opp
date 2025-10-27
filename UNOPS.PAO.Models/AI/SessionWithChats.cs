using System.Collections.Generic;
using UNOPS.PAO.Domain.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UNOPS.PAO.Models.AI
{
    public class SessionWithChats
    {
        [JsonProperty("session")]
        public AiChatSession? Session { get; set; }
        
        [JsonProperty("chatMessages")]
        public JArray ChatMessages { get; set; } = new JArray();
    }
} 