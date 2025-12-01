using UNOPS.PAO.Domain.Entities;
using System.Text.Json.Serialization;
using System.Linq;
using UNOPS.PAO.Models.OrganizationUnits;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Documents;

namespace UNOPS.PAO.Models.Partners;

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
    public string? PartnerFocalPointUserName { get; set; } // Business Developer Email (from navigation)
    public string? PartnerFocalPointName { get; set; } // Business Developer Display Name (from navigation)
    
    // Partner Group Information
    public string? PartnerGroupCode { get; set; }
    public string? PartnerGroupName { get; set; }
    public int? PartnerGroupId { get; set; }
        
    // ERP Integration
    public int? ErpDimValue { get; set; } // ERP dimension value

    // UN & State Entity
    public bool UNAndStateEntity { get; set; }

    // ========== APPROVAL FIELDS (Admin only) ==========
    public bool KeyGlobalPartner { get; set; }
    public bool UNSecretariatPartner { get; set; }
    public string? DueDiligenceRequired { get; set; } // "NotRequired" / "Required" 
    public string? DueDiligenceApproval { get; set; } // "NotApproved" / "Approved"
    public DateTime? DueDiligenceApprovalDate { get; set; }
    public DateTime? DueDiligenceExpiryDate { get; set; }
    public string PartnerApprovalStatus { get; set; } // "NotApproved" / "Approved"
    public DateTime? PartnerApprovalDate { get; set; }
    public string? PartnerApprovalReference { get; set; }
    public string? PartnerLevyStatus { get; set; } // "DoesNotApply" / "PotentiallyApplied" / "PotentiallyNotApplied"
    public string? ReasonForLevy { get; set; }
    public string? LevyTreatment { get; set; }
    public bool PooledFund { get; set; }

    // System Status
    public string Status { get; set; } // Draft / Active / Closed / Archived

    // Logo URL
    public string? LogoUrl { get; set; }

    // ========== NAVIGATION PROPERTIES ==========
    // First 5 contacts by date (computed property will be handled in mapping)
    public List<ContactModel>? First5ContactsByDate { get; set; }
    
    // Computed property from NotMapped field in Partner entity
    public string? PartnerOrgUnit { get; set; }
    
    public List<DocumentModel>? Documents { get; set; }
    
    // Organization Unit Relationships
    public List<OrganizationUnitRelationshipModel>? OrganizationUnitRelationships { get; set; }
    
    
    // ========== CONDITIONAL TAGS ==========
    public List<EntityTagModel>? Tags => CalculateConditionalTags(); // Dynamic conditional tags
    
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
    
    // Resolved user names for audit fields
    public string? CreatedByName { get; set; }
    public string? LastModifiedByName { get; set; }
    
    /// <summary>
    /// Gets the primary organization unit (first relationship)
    /// </summary>
    public OrganizationHierarchyModel? GetPrimaryOrganizationUnit()
    {
        return OrganizationUnitRelationships?.FirstOrDefault()?.OrganizationHierarchy;
    }
    
    /// <summary>
    /// Calculate conditional tags based on partner's current state for frontend display
    /// </summary>
    public List<EntityTagModel> CalculateConditionalTags()
    {
        var tags = new List<EntityTagModel>();
        
        // Partner Status Tags
        if (!string.IsNullOrEmpty(Status))
        {
            var statusColor = Status switch
            {
                "Draft" => "bg-badge-secondary text-badge-secondary",      // Gray - matches p-badge severity="secondary"
                "Active" => "bg-badge-info text-badge-info",                // Blue - matches p-badge severity="info"
                "Closed" => "bg-badge-danger text-badge-danger",            // Red - matches p-badge severity="danger"
                "Archived" => "bg-yellow-100 text-yellow-800",              // Yellow - archived state
                _ => "bg-badge-secondary text-badge-secondary"
            };
            tags.Add(new EntityTagModel { Tag = Status, Color = statusColor });
        }
        
        // Partner Approval Status Tags  
        if (!string.IsNullOrEmpty(PartnerApprovalStatus) && !string.IsNullOrEmpty(Status) && Status != "Closed" && Status != "Archived")
        {
            var approvalTag = PartnerApprovalStatus switch
            {
                "Approved" => "Approved",
                "NotApproved" => "Pending Approval",
                _ => PartnerApprovalStatus
            };
            var approvalColor = PartnerApprovalStatus switch
            {
                "Approved" => "bg-badge-success text-badge-success",       // Green - matches p-badge severity="success"
                "NotApproved" => "bg-badge-warn text-badge-warn",           // Orange - matches p-badge severity="warn"
                _ => "bg-badge-secondary text-badge-secondary"
            };
            tags.Add(new EntityTagModel { Tag = approvalTag, Color = approvalColor });
        }
        
        // Due Diligence Expiry Tags
        if (DueDiligenceExpiryDate.HasValue)
        {
            var now = DateTime.UtcNow;
            var expiryDate = DueDiligenceExpiryDate.Value;
            
            if (expiryDate < now)
            {
                // Already expired
                tags.Add(new EntityTagModel { Tag = "DD Expired", Color = "bg-badge-danger text-badge-danger" });
            }
            else if (expiryDate <= now.AddMonths(6))
            {
                // Expiring within 6 months
                tags.Add(new EntityTagModel { Tag = "DD Expiring", Color = "bg-badge-warn text-badge-warn" });
            }
        }
        
        return tags;
    }
}
