using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSDomain.Entities.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class BudgetLine : BaseBusinessEntity
{
    public string Code { get; set; }
    public Budget Budget { get; set; }
    public string NatureOfCost { get; set; }
    public string BudgetAccount { get; set; }
    public WorkPackage WorkPackage { get; set; }
    public Donor Donor { get; set; }
    public int FiscalYear { get; set; }
    public decimal Amount { get; set; }
}
