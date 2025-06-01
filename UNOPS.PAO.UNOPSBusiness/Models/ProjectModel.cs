namespace UNOPS.PAO.UNOPSBusiness.Models;

using System;
using UNOPS.PAO.Models;

public class ProjectModel
{
    public int Id { get; set; }
    public string ProjectNumber { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Stage { get; set; }
    public string BudgetCheckingLevel { get; set; }
    public string BudgetDuration { get; set; }
    public Double? BudgetAmount { get; set; }
    public Double? ExpenditureAmount { get; set; }
    
    /// <summary>
    /// Partners associated with this project through the many-to-many relationship
    /// </summary>
    public List<PartnerSummaryModel>? Partners { get; set; }
    
    /// <summary>
    /// Permissions for this specific project
    /// </summary>
    public EntityPermissionsModel? Permissions { get; set; }
}

/// <summary>
/// Simplified partner model to avoid circular references
/// </summary>
public class PartnerSummaryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string ShortName { get; set; }
    public string? PartnerGroupCode { get; set; }
    public string? PartnerGroupName { get; set; }
    public Boolean GlobalKeyAccount { get; set; }
    public Boolean UNSecretariatEntity { get; set; }
}