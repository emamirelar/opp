using UNOPS.PAO.Domain.Enums;
using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models;

public class InteractionRequest : ExtensibleModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InteractionType Type { get; set; } = InteractionType.Email;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    public string? Description { get; set; }
    
    public int ContactId { get; set; }
    public List<string>? EmailAddresses { get; set; } = new List<string>();
    public List<string>? PhoneNumbers { get; set; } = new List<string>();
    public List<int>? ContactIds { get; set; } = new List<int>();
    public List<int>? PartnerIds { get; set; } = new List<int>();
    public List<int>? UserIds { get; set; } = new List<int>();
    public string? Location { get; set; }
    public string Subject { get; set; }
    public int? OrgUnitId { get; set; }
} 