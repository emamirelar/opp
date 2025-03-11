
namespace UNOPS.PAO.UNOPSDomain.Entities.Common;

using UNOPS.PAO.Domain.Entities;

public class Project : BaseBusinessEntity
{
    public string ProjectNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Stage { get; set; }
    public string BudgetCheckingLevel { get; set; }
    public string BudgetDuration { get; set; }
    public UNOPSPartner? Partner { get; set; } 
}