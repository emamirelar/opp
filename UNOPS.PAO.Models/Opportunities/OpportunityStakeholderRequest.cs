namespace UNOPS.PAO.Models;

public class OpportunityStakeholderRequest
{
    public required string StakeholderType { get; set; }
    public int? UserId { get; set; }
    public int? EntityRoleId { get; set; }
}

