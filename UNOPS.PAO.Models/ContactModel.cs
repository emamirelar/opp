using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models;

public class ContactModel
{
    public int Id { get; set; }
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public string? Suffix { get; set; }
    public string Title { get; set; }
    public string? Department { get; set; }
    public string? Description { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Assistant { get; set; }
    public string? AssistantPhone { get; set; }
    public string? AssistantEmail { get; set; }
    public string? MailingStreet { get; set; }
    public string? MailingStreet2 { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingStateProvince { get; set; }
    public string? MailingPostalCode { get; set; }
    public string? MailingCountry { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Status { get; set; }
    
    public PartnerSummaryModel? Partner { get; set; }
    public List<DocumentModel>? Documents { get; set; }

    public string? CreatedByName { get; set; }
    public string? CreatedByOfficeName { get; set; }
    
    /// <summary>
    /// Interactions associated with this contact
    /// </summary>
    public List<InteractionModel>? Interactions { get; set; }
    
    /// <summary>
    /// Permissions for this specific contact
    /// </summary>
    public EntityPermissionsModel? Permissions { get; set; }
    
    // Audit fields from ModifiableDeletableEntity (read-only from frontend perspective)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? CreatedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? LastModifiedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CreatedBy { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? LastModifiedBy { get; set; }
}

/// <summary>
/// Simplified partner model for contact references
/// </summary>
public class PartnerSummaryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
}