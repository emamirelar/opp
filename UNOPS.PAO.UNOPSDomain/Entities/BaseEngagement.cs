using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSDomain.Entities;

public class BaseEngagement : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Will use EngagementDescription or EngagementNumber
    public EntityStatus Status { get; set; } = EntityStatus.Active; // Read-only entities default to Active
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    // Primary identifier (maps to BaseEngagement column from external service)
    public string EngagementNumber { get; set; } = string.Empty;
    
    // OpportunityPlus Integration - Link to source opportunity
    public int? OpportunityId { get; set; }
    
    // Date fields (populated by External Data Service)
    public DateTime? EngagementImplementationStartDate { get; set; }
    public DateTime? EngagementImplementationEndDate { get; set; }
    public DateTime? EngagementSignedDate { get; set; }
    
    // Financial information
    public decimal? EngagementAmount { get; set; }
    
    // Stage and status information
    public string? EngagementStage { get; set; }
    public string? EngagementStageDescription { get; set; }
    
    // Business developer information
    public string? BusinessDeveloper { get; set; }
    public string? BusinessDeveloperName { get; set; }
    public string? BusinessDeveloperEmailAddress { get; set; }
    
    // Project executive information
    public string? EngagementProjectExecutive { get; set; }
    public string? EngagementProjectExecutiveName { get; set; }
    
    // Implementation details (text fields from BigQuery)
    public string? ImplementationCountriesList { get; set; }
    public string? OutputsList { get; set; }
    public string? SDGList { get; set; }
    
    // Descriptions
    public string? EngagementDescription { get; set; }
    public string? EngagementLongDescription { get; set; }
    
    // Navigation Properties (for LINQ convenience, NO FK constraints)
    [JsonIgnore]
    public virtual ICollection<BaseEngagementPartners> EngagementPartners { get; set; } = new List<BaseEngagementPartners>();
}
