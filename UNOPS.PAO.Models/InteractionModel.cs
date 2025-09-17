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

    public string? Description { get; set; }
    
    public string? ContactName { get; set; }
    public string Status { get; set; }
    public virtual List<string>? EmailAddresses { get; set; } = new List<string>();
    public virtual List<string>? PhoneNumbers { get; set; } = new List<string>();
    public List<int>? ContactIds { get; set; } = new List<int>();
    [JsonIgnore]
    public virtual ICollection<InteractionContactModel>? InteractionContacts { get; set; }
    public List<int>? PartnerIds { get; set; } = new List<int>();
    [JsonIgnore]
    public virtual ICollection<InteractionPartnerModel>? InteractionPartners { get; set; }
    [JsonIgnore]
    public virtual ICollection<InteractionUserModel>? InteractionUsers { get; set; }
    public string? Location { get; set; }
    public string Subject { get; set; }
    
    public virtual ICollection<OrganizationUnitRelationshipModel>? OrganizationUnitRelationships { get; set; }
    
    public List<DocumentModel>? Documents { get; set; }

    /// <summary>
    /// Full contact entities associated with this interaction
    /// </summary>
    public List<ContactModel>? Contacts { get; set; }

    /// <summary>
    /// Full partner entities associated with this interaction
    /// </summary>
    public List<PartnerModel>? Partners { get; set; }

    /// <summary>
    /// Full user entities associated with this interaction
    /// </summary>
    public List<UserValueModel>? Users { get; set; }

    /// <summary>
    /// Permissions for this specific interaction
    /// </summary>
    public EntityPermissionsModel? Permissions { get; set; }
    public string? GmailThreadId { get; set; }
    public string? GmailMessageId { get; set; }
    
    // Computed properties from NotMapped fields in Interaction entity
    public string? InteractionContactsList { get; set; }
    public string? InteractionPartnersList { get; set; }
    public string? InteractionUsersList { get; set; }
    public string? InteractionOrgUnits { get; set; }
    
    // Audit fields from ModifiableDeletableEntity (read-only from frontend perspective)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? CreatedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? LastModifiedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CreatedBy { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? LastModifiedBy { get; set; }
    
    // User name fields resolved from UserProfile
    public string? CreatedByName { get; set; }
    public string? LastModifiedByName { get; set; }
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