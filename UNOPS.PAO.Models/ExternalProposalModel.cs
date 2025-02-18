namespace UNOPS.PAO.Models;
public class ExternalProposalModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public required int FundingOpportunityId { get; set; }
    public required string FundingOpportunityName { get; set; }
    public DateTime FundingOpportunitySubmissionDueDate { get; set; }
    public bool EligibilityCriteriaMet { get; set; }
    public bool EligibilityEntityMet { get; set; }
    
    public DateTime? SubmissionDate { get; set; }
    public string Stage { get; set; }
}