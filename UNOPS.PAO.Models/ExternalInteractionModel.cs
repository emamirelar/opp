using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Models;

public class ExternalInteractionModel
{
    public int Id { get; set; }
    public InteractionType Type { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }
} 