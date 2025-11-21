namespace UNOPS.PAO.Models;

/// <summary>
/// Model for external stakeholders (contacts) associated with an opportunity
/// </summary>
public class OpportunityExternalStakeholderModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactOrganization { get; set; }
}

