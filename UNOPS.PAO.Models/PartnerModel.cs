using UNOPS.PAO.Domain.Entities;
using System.Text.Json.Serialization;
using System.Linq;

namespace UNOPS.PAO.Models;

public class PartnerModel
{
    // ========== SYSTEM GENERATED KEYS ==========
    public int Id { get; set; } // Partner ID - system-generated
    public Guid UniqueKey { get; set; } // System Generated
    public Guid PartnerKey { get; set; } // System Generated
    public Guid PartnerCategoryInternalKey { get; set; } // System Generated
    public Guid PartnerCategoryKey { get; set; } // System Generated
    public Guid PartnerTypeKey { get; set; } // System Generated
    

    
    // ========== MAIN PARTNER FIELDS ==========
    public string Name { get; set; } // Partner name - primary identifier
    public string? PartnerShortDescription { get; set; } // Short name or acronym (optional)
    public string? PartnerLongDescription { get; set; } // Optional long description

    // Category & Org Unit
    public int? PartnerCategoryId { get; set; } // FK to Partner Category (optional)
    public string? PartnerCategoryName { get; set; }
    public string? PartnerCategoryCode { get; set; } // Partner category code

    public int? LiaisonOfficeId { get; set; } // FK to LiaisonOffice (optional)
    public string? LiaisonOfficeName { get; set; } // Navigation property
    
    // Partner Focal Point  
    public int? PartnerFocalPointUserId { get; set; } // Business Developer UserId
    public string? PartnerFocalPointUserName { get; set; } // Business Developer Name (from navigation)
    
    // Partner Group Information
    public string? PartnerGroupCode { get; set; }
    public string? PartnerGroupName { get; set; }
    public int? PartnerGroupId { get; set; }
    

    
    // ========== PARTNER LEVEL INFORMATION ==========
    public string? PartnerLevelCode { get; set; } // Imported from BQ
    public string? PartnerLevelShort { get; set; } // Imported from the Partner Tree
    public string? PartnerLevelDescription { get; set; } // Imported from the Partner Tree
    
    // ERP Integration
    public int? ErpDimValue { get; set; } // ERP dimension value

    // UN & State Entity
    public bool UNAndStateEntity { get; set; }

    // ========== APPROVAL FIELDS (Admin only) ==========
    public bool KeyGlobalPartner { get; set; }
    public bool UNSecretariatPartner { get; set; }
    public string DueDiligenceRequired { get; set; } // "NotRequired" / "Required" 
    public string DueDiligenceApproval { get; set; } // "NotApproved" / "Approved"
    public DateTime? DueDiligenceApprovalDate { get; set; }
    public DateTime? DueDiligenceExpiryDate { get; set; }
    public string PartnerApprovalStatus { get; set; } // "NotApproved" / "Approved"
    public DateTime? PartnerApprovalDate { get; set; }
    public string? PartnerApprovalReference { get; set; }
    public string PartnerLevyStatus { get; set; } // "DoesNotApply" / "PotentiallyApplied" / "PotentiallyNotApplied"
    public string? ReasonForLevy { get; set; }
    public string? LevyTreatment { get; set; }
    public bool PooledFund { get; set; }
    public bool CanCreateNewOpportunities { get; set; }
    public string? ReasonForNoNewOpportunity { get; set; }

    // System Status
    public string Status { get; set; } // Draft / Active / Closed / Archived

    // Logo URL
    public string? LogoUrl { get; set; }

    // ========== NAVIGATION PROPERTIES ==========
    // First 5 contacts by date (computed property will be handled in mapping)
    public List<ContactModel>? First5ContactsByDate { get; set; }
    
    public List<DocumentModel>? Documents { get; set; }
    
    // Organization Unit Relationships
    public List<OrganizationUnitRelationshipModel>? OrganizationUnitRelationships { get; set; }
    
    /// <summary>
    /// Projects associated with this partner through the many-to-many relationship
    /// </summary>
    public List<ProjectSummaryModel>? Projects { get; set; }
    
    /// <summary>
    /// Permissions for this specific partner
    /// </summary>
    public EntityPermissionsModel? Permissions { get; set; }
    public List<InteractionModel>? Interactions { get; set; }
    public List<ContactModel>? Contacts { get; set; }
    
    // Audit fields from ModifiableDeletableEntity (read-only from frontend perspective)
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? CreatedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime? LastModifiedDate { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CreatedBy { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? LastModifiedBy { get; set; }
    
    /// <summary>
    /// Gets the primary organization unit (first relationship)
    /// </summary>
    public OrganizationHierarchyModel? GetPrimaryOrganizationUnit()
    {
        return OrganizationUnitRelationships?.FirstOrDefault()?.OrganizationHierarchy;
    }
}

/// <summary>
/// Simplified project model to avoid circular references
/// </summary>
public class ProjectSummaryModel
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
}   