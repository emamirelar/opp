namespace UNOPS.PAO.Models;

public class InternalProposalModel : ExtensibleModel
{
    public int Id { get; set; }

    public required int FundingOpportunityId { get; set; }
    public required string FundingOpportunityName { get; set; }
    public ApplicantModel? Applicant { get; set; }

    public bool EligibilityCriteriaMet { get; set; }
    public bool EligibilityEntityMet { get; set; }

    public DateTime? SubmissionDate { get; set; }
    public string Stage { get; set; }
}

