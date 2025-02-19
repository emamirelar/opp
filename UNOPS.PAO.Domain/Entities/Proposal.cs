using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;
public class Proposal : ModifiableDeletableEntity
{
    public int FundingOpportunityId { get; set; }
    public FundingOpportunity FundingOpportunity { get; set; }

    public int ApplicantId {  get; set; }
    public GrantUser Applicant { get; set; }
    public bool EligibilityCriteriaMet { get; set; }
    public bool EligibilityEntityMet { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public string Stage { get; set; } = "Draft";
    public List<Document>? Documents { get; set; }
}