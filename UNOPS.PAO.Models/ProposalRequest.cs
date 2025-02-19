namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

public class ProposalRequest : ExtensibleModel
{
    public string? Name { get; set; }
    public int FundingOpportunityId { get; set; }
    public bool EligibilityCriteriaMet { get; set; }
    public bool EligibilityEntityMet { get; set; }
    public string Stage { get; set; }
}
