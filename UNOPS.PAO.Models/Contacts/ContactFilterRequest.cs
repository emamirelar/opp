using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Domain.Specifications.Interfaces;

namespace UNOPS.PAO.Models.Contacts;

/// <summary>
/// Simplified request model for contact advanced search with pagination support
/// </summary>
public class ContactFilterRequest : PaginationRequest, IContactSearchFilter
{
    /// <summary>
    /// General search text to search across all contact fields
    /// </summary>
    public string? SearchText { get; set; }
    
    /// <summary>
    /// Contact ID for filtering
    /// </summary>
    public int? Id { get; set; }
    
    /// <summary>
    /// First name filter
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Last name filter
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Email filter
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Title filter
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Department filter
    /// </summary>
    public string? Department { get; set; }
    
    /// <summary>
    /// Phone filter
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// Mobile filter
    /// </summary>
    public string? Mobile { get; set; }
    
    /// <summary>
    /// Assistant filter
    /// </summary>
    public string? Assistant { get; set; }
    
    /// <summary>
    /// Assistant email filter
    /// </summary>
    public string? AssistantEmail { get; set; }
    
    /// <summary>
    /// Assistant phone filter
    /// </summary>
    public string? AssistantPhone { get; set; }
    
    /// <summary>
    /// Mailing city filter
    /// </summary>
    public string? MailingCity { get; set; }
    
    /// <summary>
    /// Mailing state/province filter
    /// </summary>
    public string? MailingStateProvince { get; set; }
    
    /// <summary>
    /// Mailing postal code filter
    /// </summary>
    public string? MailingPostalCode { get; set; }
    
    /// <summary>
    /// Mailing country filter
    /// </summary>
    public string? MailingCountry { get; set; }
    
    /// <summary>
    /// Partner ID filter
    /// </summary>
    public int? PartnerId { get; set; }
    
    /// <summary>
    /// Partner name filter
    /// </summary>
    public string? PartnerName { get; set; }
    
    /// <summary>
    /// Organization Unit filter - filters results by organizational unit (includes hierarchy)
    /// </summary>
    public int? OrgUnitId { get; set; }
    
    /// <summary>
    /// Indicates if advanced search is enabled
    /// </summary>
    public bool AdvancedSearch { get; set; }
    
    /// <summary>
    /// JSON string containing the search criteria
    /// </summary>
    public string? SearchCriteria { get; set; }
} 