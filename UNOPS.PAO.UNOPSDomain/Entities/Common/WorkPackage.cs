namespace UNOPS.PAO.UNOPSDomain.Entities.Common;

using System;
using UNOPS.PAO.Domain.Entities;

public class WorkPackage : BaseBusinessEntity
{
    public string Code { get; set; }
    public Project Project { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
