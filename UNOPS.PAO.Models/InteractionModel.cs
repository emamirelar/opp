using UNOPS.PAO.Domain.Enums;
using System.Text.Json.Serialization;
using System.Text;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Models;

public class InteractionModel
{
    public int Id { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InteractionType Type { get; set; }
    public DateTime Date { get; set; }

    [JsonIgnore]
    public byte[]? Data { get; set; }
    
    [JsonPropertyName("data")]
    public string? TextData 
    { 
        get => Data != null ? Encoding.UTF8.GetString(Data) : null;
        set => Data = value != null ? Encoding.UTF8.GetBytes(value) : null;
    }
    
    public int ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    [JsonIgnore]
    public virtual List<string>? EmailAddresses { get; set; } = new List<string>();
    [JsonIgnore]
    public virtual List<string>? PhoneNumbers { get; set; } = new List<string>();
    public List<int>? ContactIds { get; set; }
    [JsonIgnore]
    public virtual ICollection<InteractionContactModel>? InteractionContacts { get; set; }
    public List<int>? PartnerIds { get; set; }
    // Many-to-many with Partners
    [JsonIgnore]
    public virtual ICollection<InteractionPartnerModel>? InteractionPartners { get; set; }
    public List<int>? UserIds { get; set; }
    // Many-to-many with Users
    [JsonIgnore]
    public virtual ICollection<InteractionUserModel>? InteractionUsers { get; set; }
    public string? Location { get; set; }

    public string Subject { get; set; }

    [JsonIgnore]
    public virtual OrganizationUnitModel? OrgUnit { get; set; }

    public int? OrgUnitId { get; set; }

    public List<DocumentModel>? Documents { get; set; }
}

public class InteractionContactModel
{
    public int InteractionId { get; set; }
    public int ContactId { get; set; }
}

public class InteractionPartnerModel
{
    public int InteractionId { get; set; }
    public int PartnerId { get; set; }
}

public class InteractionUserModel
{
    public int InteractionId { get; set; }
    public int UserId { get; set; }
}