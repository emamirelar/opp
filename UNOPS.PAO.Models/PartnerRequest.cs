namespace UNOPS.PAO.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;

public class PartnerRequest : ExtensibleModel
{
    // ========== ENHANCED PARTNER FIELDS ==========
    
    // Core Partner Information
    public string PartnerDescription { get; set; } // Full name (required)
    public string PartnerShortDescription { get; set; } // Short name/acronym (required)
    public string? PartnerLongDescription { get; set; } // Optional long description
    
    // Category & Organization Unit
    public int PartnerCategoryId { get; set; } // FK to Partner Category (required)
    public int? PartnerOrgUnitId { get; set; } // Nullable if unmanaged
    public int LiaisonOfficeId { get; set; } // FK to LiaisonOffice (required)
    
    // Report Levels
    public int? PartnerInternalReportLevel { get; set; } // 1-5 defined by Partner group Hierarchy
    public int? PartnerExternalReportLevel { get; set; } // External reporting level
    
    // Partner Level Information
    public string? PartnerLevelCode { get; set; } // Imported from BQ
    public string? PartnerLevelShort { get; set; } // Imported from the Partner Tree
    public string? PartnerLevelDescription { get; set; } // Imported from the Partner Tree
    public int? ErpDimValue { get; set; } // ERP dimension value
    
    // Liaison Office
    public string? PartnerLiaisonOffice { get; set; } // Enum from predefined list (required)
    
    // UN & State Entity
    public bool UNAndStateEntity { get; set; } = false;
    
    // ========== APPROVAL FIELDS (Admin only) ==========
    public bool KeyGlobalPartner { get; set; } = false;
    public bool UNSecretariatPartner { get; set; } = false;
    
    // Due Diligence Fields
    public string DueDiligenceRequired { get; set; } = "NotRequired"; // "NotRequired" / "Required"
    public string DueDiligenceApproval { get; set; } = "NotApproved"; // "NotApproved" / "Approved"
    public DateTime? DueDiligenceApprovalDate { get; set; }
    public DateTime? DueDiligenceExpiryDate { get; set; }
    
    // Partner Approval Status
    public string PartnerApprovalStatus { get; set; } = "NotApproved"; // "NotApproved" / "Approved"
    public DateTime? PartnerApprovalDate { get; set; }
    public string? PartnerApprovalReference { get; set; }
    
    // Levy Fields
    public string PartnerLevyStatus { get; set; } = "DoesNotApply"; // "DoesNotApply" / "PotentiallyApplied" / "PotentiallyNotApplied"
    public string? ReasonForLevy { get; set; }
    public string? LevyTreatment { get; set; }
    
    // Operational Fields
    public bool PooledFund { get; set; } = false;
    public bool CanCreateNewOpportunities { get; set; } = true;
    public string? ReasonForNoNewOpportunity { get; set; }
    
    // Partner Status
    public string Status { get; set; } = "Draft"; // "Draft" / "Active" / "Closed" / "Archived"
    
    // Partner Group
    public string? PartnerGroupCode { get; set; }
    
    /// <summary>
    /// Organization unit hierarchy IDs - managed automatically by the partner manager
    /// </summary>
    public List<int>? OrganizationHierarchyIds { get; set; }
}