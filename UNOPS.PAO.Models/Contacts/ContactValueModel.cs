using UNOPS.PAO.Models.Partners;

namespace UNOPS.PAO.Models.Contacts;

public class ContactValueModel
{
    public int Id { get; set; }
    public string? Salutation { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public string? Suffix { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public int? PartnerId { get; set; }
    public PartnerValueModel Partner { get; set; }
    public string Name => string.Join(" ", new[]
    {
        Salutation,
        FirstName,
        MiddleName,
        LastName,
        Suffix
    }.Where(s => !string.IsNullOrWhiteSpace(s)));
}