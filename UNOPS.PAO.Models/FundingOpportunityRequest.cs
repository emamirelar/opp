namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class FundingOpportunityRequest : ExtensibleModel
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string EligibilityCriteria { get; set; } = string.Empty;
    public bool SingleSubmition { get; set; }

    public string? CurrencyCode { get; set; }
    public decimal FundingAvailable { get; set; }
    public int SelectionMethodologyId { get; set; }
    public string? ApplicationTypeCode { get; set; }
    public List<int>? EligibleEntityIds { get; set; }
    public List<int>? SDGIds { get; set; }
    public List<int>? CountryIds { get; set; }

    public DateTime? SubmissionDueDate { get; set; }
    public DateTime? ClarificationDeadline { get; set; }
    public DateTime? InformationSessionDate { get; set; }
    public DateTime? DecisionDate { get; set; }
}