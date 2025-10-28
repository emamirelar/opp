using UNOPS.PAO.Domain.Enums;
using System.Text.Json.Serialization;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Converters;
using Newtonsoft.Json;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Models.Interactions;

public class InteractionRequest : ExtensibleModel
{
    public InteractionType Type { get; set; } = InteractionType.Email;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    public string? Description { get; set; }
    [Newtonsoft.Json.JsonConverter(typeof(StringOrStringArrayConverter))]
    public List<string>? EmailAddresses { get; set; } = new List<string>();

    public List<int>? ContactIds { get; set; } = new List<int>();
    public List<int>? PartnerIds { get; set; } = new List<int>();
    public List<int>? UserIds { get; set; } = new List<int>();
    public string? Location { get; set; }
    public string Subject { get; set; }
    
    /// <summary>
    /// Organization unit hierarchy IDs - managed automatically by the interaction manager
    /// </summary>
    public List<int>? OrganizationHierarchyIds { get; set; }
    
    public string? GmailThreadId { get; set; }
    public string? GmailMessageId { get; set; }
    public string? Status { get; set; } = "Active";

    /// <summary>
    /// Flag to bypass duplicate detection when user confirms creation despite duplicates
    /// </summary>
    public bool ConfirmDuplicateCreation { get; set; } = false;
    public int? CreatedBy { get; set; }
}