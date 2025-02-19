using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities;

public class FundingOpportunity : ModifiableDeletableEntity
{
    public string Description { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string Stage { get; set; } = "Draft";
    public string Justification { get; set; } = string.Empty;
    public string EligibilityCriteria { get; set; } = string.Empty;
    public bool SingleSubmition { get; set; }
    public List<SDG>? SDGs { get; set; }
    public List<Country>? Countries { get; set; }
    public Currency? Currency { get; set; }
    public decimal FundingAvailable { get; set; }
    public SelectionMethodology? SelectionMethodology { get; set; }
    public List<Proposal>? Proposals { get; set; }
    public List<Document>? Documents { get; set; }
    public string? ApplicationTypeCode { get; set; }
    public List<EligibleEntity>? EligibleEntities { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? SubmissionDueDate { get; set; }
    public DateTime? ClarificationDeadline { get; set; }
    public DateTime? InformationSessionDate { get; set; }
    public DateTime? DecisionDate { get; set; }
}

