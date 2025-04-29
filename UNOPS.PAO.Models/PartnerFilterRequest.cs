using UNOPS.PAO.Models;

public class PartnerFilterRequest : PaginationRequest
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? NewEngagement { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ShortName { get; set; }
    public int? PartnerOfficeId { get; set; }
    public int? PartnerCategoryId { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressStateProvince { get; set; }
    public string? AddressPostalCode { get; set; }
    public string? AddressCountry { get; set; }
    public string? SearchText { get; set; }
} 