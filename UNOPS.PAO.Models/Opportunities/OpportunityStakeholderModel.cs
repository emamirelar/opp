namespace UNOPS.PAO.Models;

public class OpportunityStakeholderModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public int EntityRoleId { get; set; }
    public string? EntityRoleName { get; set; }
    public string? StakeholderType { get; set; }
    public bool IsInternal { get; set; }
    public string? UserEmail { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? Organization { get; set; }
    public string? Notes { get; set; }
}

