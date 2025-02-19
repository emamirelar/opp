using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;

public class UNOPSFundingOpportunity : Domain.Entities.FundingOpportunity
{
    public string ProjectNumber { get; set; }
    public Project Project { get; set; }
    public List<Document> Documents { get; set; }
}
