using UNOPS.PAO.Models;

public class ContactFilterRequest : PaginationRequest
{
    public int? Id { get; set; }
    public int? PartnerId { get; set; }
    public string? Status { get; set; }
    public string? Salutation { get; set; }
    public string? Title { get; set; }
    public string? Department { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Assistant { get; set; }
    public string? AssistantEmail { get; set; }
    public string? AssistantPhone { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingStateProvince { get; set; }
    public string? MailingPostalCode { get; set; }
    public string? MailingCountry { get; set; }
    public string? SearchText { get; set; }
} 