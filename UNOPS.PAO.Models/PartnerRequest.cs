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
    
    // Core Partner Information - using Name field (inherited from base model)
    public string? Name { get; set; } // Partner name - primary identifier (optional)
    public string? PartnerShortDescription { get; set; } // Short name/acronym (optional)
    public string? PartnerLongDescription { get; set; } // Optional long description
    
    // Category & Organization Unit
    public int? PartnerCategoryId { get; set; } // FK to Partner Category (optional)

    public int? LiaisonOfficeId { get; set; } // FK to LiaisonOffice (optional)
    
    // Partner Focal Point
    public int? PartnerFocalPointUserId { get; set; } // Business Developer UserId

    public int? ErpDimValue { get; set; } // ERP dimension value
    
    // Liaison Office - handled via LiaisonOfficeId above
    
    // UN & State Entity
    public bool UNAndStateEntity { get; set; } = false;
    
    // ========== APPROVAL FIELDS (Admin only) ==========
    public bool KeyGlobalPartner { get; set; } = false;
    public bool UNSecretariatPartner { get; set; } = false;
    
    // Due Diligence Fields
    public string? DueDiligenceRequired { get; set; } // "NotRequired" / "Required"
    public string? DueDiligenceApproval { get; set; } // "NotApproved" / "Approved"
    public DateTime? DueDiligenceApprovalDate { get; set; }
    public DateTime? DueDiligenceExpiryDate { get; set; }
    
    // Partner Approval Status
    public string? PartnerApprovalStatus { get; set; } // "NotApproved" / "Approved"
    public DateTime? PartnerApprovalDate { get; set; }
    public string? PartnerApprovalReference { get; set; }
    
    // Levy Fields
    public string? PartnerLevyStatus { get; set; } // "DoesNotApply" / "PotentiallyApplied" / "PotentiallyNotApplied"
    public string? ReasonForLevy { get; set; }
    public string? LevyTreatment { get; set; }
    
    // Operational Fields
    public bool PooledFund { get; set; } = false;
    public bool CanCreateNewOpportunities { get; set; }
    public string? ReasonForNoNewOpportunity { get; set; }
    
    // Partner Status
    public string? Status { get; set; } // "Draft" / "Active" / "Closed" / "Archived"
    
    // Partner Group
    public string? PartnerGroupCode { get; set; }
    
    /// <summary>
    /// Organization unit hierarchy IDs - managed automatically by the partner manager
    /// </summary>
    public List<int>? OrganizationHierarchyIds { get; set; }
}



public class StatusChangeRequest  
{
    public string Status { get; set; } = string.Empty; // Draft, Active, Closed, Archived
    public string? Notes { get; set; }
}

public class ActivatePartnerRequest
{
    public string? Notes { get; set; }
}

public class PartnerValidationResult
{
    public bool IsValid { get; set; }
    public List<string> MissingFields { get; set; } = new();
    public bool CanBeActivated { get; set; }
    public string? ValidationMessage { get; set; }
}