namespace UNOPS.PAO.Models;

public class OpportunityStakeholderModel
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int EntityRoleId { get; set; }
    public string? EntityRoleName { get; set; }
    public bool IsInternal { get; set; }
    public string? StakeholderType { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string? Notes { get; set; }
}
