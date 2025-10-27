namespace UNOPS.PAO.Models.AI;
public class AiChatSessionModel
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = "Active";
    public bool TextToSpeech { get; set; } = false;
}