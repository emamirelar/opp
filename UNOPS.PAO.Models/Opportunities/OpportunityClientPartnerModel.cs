namespace UNOPS.PAO.Models;

public class OpportunityClientPartnerModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int PartnerId { get; set; }
    public string? PartnerName { get; set; }
}

