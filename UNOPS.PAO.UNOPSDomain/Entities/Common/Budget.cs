namespace UNOPS.PAO.UNOPSDomain.Entities.Common;

using UNOPS.PAO.Domain.Entities;

public class Budget : BaseBusinessEntity
{
    public Project Project { get; set; }
    public string Version { get; set; }
    public string BudgetVersion { get; set; }
    public string BudgetNumber { get; set; }
}
