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
    public PartnerModel Partner { get; set; }
    public int? PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public List<DocumentModel>? Documents { get; set; }
}