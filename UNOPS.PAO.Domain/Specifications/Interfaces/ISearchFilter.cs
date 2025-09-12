namespace UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// Interface for pagination and ordering functionality
/// </summary>
public interface IPaginationFilter
{
    string? OrderBy { get; set; }
    bool? Ascending { get; set; }
}

/// <summary>
/// Interface for basic search functionality
/// </summary>
public interface ISearchFilter : IPaginationFilter
{
    string? SearchText { get; set; }
}

/// <summary>
/// Interface for organizational unit filtering
/// </summary>
public interface IOrgUnitFilter
{
    int? OrgUnitId { get; set; }
}

/// <summary>
/// Interface for contact-related search functionality
/// </summary>
public interface IContactSearchFilter : ISearchFilter, IOrgUnitFilter
{
    int? Id { get; set; }
    string? FirstName { get; set; }
    string? LastName { get; set; }
    string? Email { get; set; }
    string? Title { get; set; }
    string? Department { get; set; }
    string? Phone { get; set; }
    string? Mobile { get; set; }
    string? Assistant { get; set; }
    string? AssistantEmail { get; set; }
    string? AssistantPhone { get; set; }
    string? MailingCity { get; set; }
    string? MailingStateProvince { get; set; }
    string? MailingPostalCode { get; set; }
    string? MailingCountry { get; set; }
    int? PartnerId { get; set; }
    string? PartnerName { get; set; }
    
    // Advanced search properties
    bool AdvancedSearch { get; set; }
    string? SearchCriteria { get; set; }
}

/// <summary>
/// Interface for partner-related search functionality
/// </summary>
public interface IPartnerSearchFilter : ISearchFilter, IOrgUnitFilter
{
    int? Id { get; set; }
    string? Name { get; set; }
    string? Status { get; set; }
    string? NewEngagement { get; set; }
    string? Phone { get; set; }
    string? Website { get; set; }
    string? ShortName { get; set; }
    int? OrganizationHierarchyId { get; set; }
    string? OrganizationHierarchyName { get; set; }
    int? PartnerCategoryId { get; set; }
    string? PartnerCategoryName { get; set; }
    string? AddressCity { get; set; }
    string? AddressStateProvince { get; set; }
    string? AddressPostalCode { get; set; }
    string? AddressCountry { get; set; }
    int? PartnerGroupId { get; set; }
}

/// <summary>
/// Interface for interaction-related search functionality
/// </summary>
public interface IInteractionSearchFilter : ISearchFilter, IOrgUnitFilter
{
    int? Id { get; set; }
    int? ContactId { get; set; }
    string? ContactName { get; set; }
    int? PartnerId { get; set; }
    string? Type { get; set; }
    DateTime? FromDate { get; set; }
    DateTime? ToDate { get; set; }
    DateTime? Date { get; set; }
    string? Description { get; set; }
    string? Subject { get; set; }
    
    // Advanced search properties
    bool AdvancedSearch { get; set; }
    string? SearchCriteria { get; set; }
} 