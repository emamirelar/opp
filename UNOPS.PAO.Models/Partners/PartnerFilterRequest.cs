using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Models.Shared;

public class PartnerFilterRequest : PaginationRequest, IPartnerSearchFilter
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? NewEngagement { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ShortName { get; set; }
    public int? OrganizationHierarchyId { get; set; }
    public string? OrganizationHierarchyName { get; set; }
    public int? PartnerCategoryId { get; set; }
    public string? PartnerCategoryName { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressStateProvince { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; }
    public int? PartnerGroupId { get; set; }
    public string? SearchText { get; set; }
    
    // Organization Unit filter - filters results by organizational unit (includes hierarchy)
    public int? OrgUnitId { get; set; }
    
    // Advanced search properties
    public bool AdvancedSearch { get; set; }
    public string? SearchCriteria { get; set; }
} 