namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

public class ExternalFundingOpportunityModel : ExtensibleModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string Stage { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string EligibilityCriteria { get; set; } = string.Empty;
    public bool SingleSubmition { get; set; }

    public ApplicationTypeModel? ApplicationType { get; set; }

    public SelectonMethodologyModel? SelectionMethodology { get; set; }
    public List<EligibleEntityModel>? EligibleEntities { get; set; }
    public List<SDGModel>? SDGs { get; set; }
    public List<CountryModel> Countries { get; set; }
    public List<DocumentModel>? Documents { get; set; }
    public CurrencyModel? Currency { get; set; }
    public decimal FundingAvailable { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? SubmissionDueDate { get; set; }
    public DateTime? ClarificationDeadline { get; set; }
    public DateTime? InformationSessionDate { get; set; }
}