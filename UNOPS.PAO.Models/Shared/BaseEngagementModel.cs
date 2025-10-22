using System.Text.Json.Serialization;

namespace UNOPS.PAO.Models.Shared;

public class BaseEngagementModel
{
    public int Id { get; set; }
    
    // Primary identifier
    public string EngagementNumber { get; set; } = string.Empty;
    
    // Date fields
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
    
    // Implementation details
    public string? ImplementationCountriesList { get; set; }
    public string? OutputsList { get; set; }
    public string? SDGList { get; set; }
    
    // Descriptions
    public string? EngagementDescription { get; set; }
    public string? EngagementLongDescription { get; set; }
    
    // Partner relationship data (populated by joins)
    public List<BaseEngagementPartnerModel> Partners { get; set; } = new();
    public int PartnerCount => Partners.Count;
    
    // Permissions for this specific engagement
    public EntityPermissionsModel? Permissions { get; set; }
    
    // Audit fields from ModifiableDeletableEntity (read-only from frontend perspective)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? CreatedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? LastModifiedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CreatedBy { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? LastModifiedBy { get; set; }
    
    // Display helpers
    public string DisplayName => !string.IsNullOrEmpty(EngagementDescription) 
        ? EngagementDescription 
        : EngagementNumber;
    
    public string StageDisplay => EngagementStageDescription ?? EngagementStage ?? "Unknown";
    
    public string DurationDisplay => EngagementImplementationEndDate.HasValue 
        ? $"{EngagementImplementationStartDate?.ToString("MMM yyyy") ?? "TBD"} - {EngagementImplementationEndDate?.ToString("MMM yyyy")}" 
        : EngagementImplementationStartDate.HasValue 
            ? $"Since {EngagementImplementationStartDate?.ToString("MMM yyyy")}"
            : "Duration TBD";
    
    public string BudgetDisplay => EngagementAmount.HasValue 
        ? $"{EngagementAmount:C}" 
        : "Budget not specified";
    
    public string BusinessDeveloperDisplay => !string.IsNullOrEmpty(BusinessDeveloperName)
        ? BusinessDeveloperName
        : BusinessDeveloper ?? "Not assigned";
}

public class BaseEngagementPartnerModel
{
    public int Id { get; set; }
    
    // Primary identifier
    public string Key { get; set; } = string.Empty;
    
    // Engagement reference
    public string EngagementNumber { get; set; } = string.Empty;
    
    // Partner information (from source)
    public string? PartnerType { get; set; }
    public string? Partner { get; set; }
    public string? PartnerDescription { get; set; }
    
    // Resolved foreign key IDs
    public int? PartnerId { get; set; }
    public int? BaseEngagementId { get; set; }
    
    // Related data (populated by joins)
    public string EngagementDescription { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    
    // Permissions for this specific engagement partner relationship
    public EntityPermissionsModel? Permissions { get; set; }
    
    // Audit fields from ModifiableDeletableEntity (read-only from frontend perspective)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? CreatedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? LastModifiedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CreatedBy { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? LastModifiedBy { get; set; }
    
    // Display helpers
    public string PartnerTypeDisplay => PartnerType?.Replace("_", " ") ?? "Partner";
    public string PartnerDisplayName => !string.IsNullOrEmpty(PartnerDescription) 
        ? PartnerDescription 
        : PartnerName ?? Partner ?? "Unknown Partner";
}
