namespace UNOPS.PAO.Models;
public class UpdateProposalRequest
{
    public int Id { get; set; }
    public bool EligibilityCriteriaMet { get; set; }
    public bool EligibilityEntityMet { get; set; }

}