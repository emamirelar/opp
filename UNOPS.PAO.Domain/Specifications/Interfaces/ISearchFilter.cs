namespace UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// Interface for basic search functionality
/// </summary>
public interface ISearchFilter
{
    string? SearchText { get; set; }
}

/// <summary>
/// Interface for partner-related search functionality
/// </summary>
public interface IPartnerSearchFilter : ISearchFilter
{
    int? Id { get; set; }
    string? Name { get; set; }
    string? Status { get; set; }
    string? NewEngagement { get; set; }
    string? Phone { get; set; }
    string? Website { get; set; }
    string? ShortName { get; set; }
    int? PartnerOfficeId { get; set; }
    string? PartnerOfficeName { get; set; }
    int? PartnerCategoryId { get; set; }
    string? PartnerCategoryName { get; set; }
    string? AddressCity { get; set; }
    string? AddressStateProvince { get; set; }
    string? AddressPostalCode { get; set; }
    string? AddressCountry { get; set; }
}

/// <summary>
/// Interface for contact-related search functionality
/// </summary>
public interface IContactSearchFilter : ISearchFilter
{
    int? Id { get; set; }
    int? PartnerId { get; set; }
    string? Status { get; set; }
    string? Salutation { get; set; }
    string? FirstName { get; set; }
    string? LastName { get; set; }
    string? MiddleName { get; set; }
    string? Title { get; set; }
    string? Email { get; set; }
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
    string? PartnerName { get; set; }
    string? PartnerStatus { get; set; }
    string? PartnerShortName { get; set; }
} 