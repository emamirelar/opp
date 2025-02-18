namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;

public class ProposalModel : ExtensibleModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int FundingOpportunityId { get; set; }
    public FundingOpportunityModel FundingOpportunity { get; set; }
    public int ApplicantId { get; set; }
    public ApplicantModel Applicant { get; set; }
    public bool EligibilityCriteriaMet { get; set; }
    public bool EligibilityEntityMet { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public string Stage { get; set; }
    public List<DocumentModel>? Documents { get; set; }
}
