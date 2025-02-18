namespace UNOPS.PAO.UNOPSBusiness.Models;

using System;

public class ProjectModel
{
    public string ProjectNumber { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Stage { get; set; }
    public string BudgetCheckingLevel { get; set; }
    public string BudgetDuration { get; set; }
}