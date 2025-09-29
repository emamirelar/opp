using System.Text.Json.Serialization;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSDomain.Entities;

public class BaseEngagementPartners : IBaseBusinessEntity<int>
{
    // IBaseBusinessEntity requirements
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Will use Key or combination
    public EntityStatus Status { get; set; } = EntityStatus.Active; // Read-only entities default to Active
    
    // Audit field (managed by External Data Service)
    public bool IsDeleted { get; set; } = false;
    
    // Primary identifier (maps to Key from BigQuery)
    public string Key { get; set; } = string.Empty;
    
    // Engagement reference (maps to Base_Engagement from BigQuery)
    public string EngagementNumber { get; set; } = string.Empty;
    
    // Partner information (from BigQuery source fields)
    public string? PartnerType { get; set; }
    public string? Partner { get; set; }
    public string? PartnerDescription { get; set; }
    
    // Foreign Key IDs (resolved by External Data Service from lookup mappings)
    // Partner -> Partners.ErpDimValue -> PartnerId (Partners.Id)
    public int? PartnerId { get; set; }
    // Base_Engagement -> BaseEngagements.BaseEngagement -> BaseEngagementId (BaseEngagements.Id)
    public int? BaseEngagementId { get; set; }
    
    // Navigation Properties (for LINQ convenience, NO FK constraints)
    // These allow joins in LINQ queries but create no database relationships
    [JsonIgnore]
    public virtual BaseEngagement? BaseEngagementEntity { get; set; }
    
    [JsonIgnore]
    public virtual UNOPSPartner? PartnerEntity { get; set; }
}
