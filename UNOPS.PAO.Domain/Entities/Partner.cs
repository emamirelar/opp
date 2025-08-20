using System.Collections.Generic;
using System;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace UNOPS.PAO.Domain.Entities;

public class Partner : ModifiableDeletableEntity
{
    public int Id { get; set; }
    public string? LogoUrl { get; set; }
    public List<Document>? Documents { get; set; }
    
    // Navigation property for organization unit relationships
    public virtual ICollection<OrganizationUnitRelationship> OrganizationUnitRelationships { get; set; } = new HashSet<OrganizationUnitRelationship>();
    
    [ForeignKey("PartnerGroupCode")]
    public PartnerTree? PartnerGroup { get; set; }
    
    public string? PartnerGroupCode { get; set; }

    // ========== SYSTEM GENERATED KEYS ==========
    
    // System-generated unique identifiers
    public Guid UniqueKey { get; set; } = Guid.NewGuid(); // System Generated
    public Guid PartnerKey { get; set; } = Guid.NewGuid(); // System Generated
    public Guid PartnerCategoryInternalKey { get; set; } = Guid.NewGuid(); // System Generated
    public Guid PartnerCategoryKey { get; set; } = Guid.NewGuid(); // System Generated
    public Guid PartnerTypeKey { get; set; } = Guid.NewGuid(); // System Generated
    
    // ========== ENHANCED PARTNER FIELDS ==========
    
    // Enhanced descriptions - using inherited Name field as primary identifier
    [MaxLength(100)]  
    public string? PartnerShortDescription { get; set; } // Short name/acronym (optional)
    
    [MaxLength(1000)]
    public string? PartnerLongDescription { get; set; } // Optional long description
    
    // Category (Organization Units are managed through OrganizationUnitRelationships)
    public int? PartnerCategoryId { get; set; } // FK to Partner Category (optional)
    
    // ERP Integration
    public int? ErpDimValue { get; set; } // ERP dimension value
    
    // Liaison Office  
    public int? LiaisonOfficeId { get; set; } // FK to LiaisonOffice (optional)
    
    // Partner Focal Point - Selection from active directory to serve as partner team  
    public int? PartnerFocalPointUserId { get; set; } // Business Developer UserId
    
    // UN & State Entity
    public bool UNAndStateEntity { get; set; } = false;
    
    // ========== APPROVAL FIELDS (Admin only) ==========
    public bool KeyGlobalPartner { get; set; } = false;
    public bool UNSecretariatPartner { get; set; } = false;
    
    // Due Diligence Fields
    public DueDiligenceRequired DueDiligenceRequired { get; set; }
    public DueDiligenceApproval DueDiligenceApproval { get; set; }
    public DateTime? DueDiligenceApprovalDate { get; set; }
    public DateTime? DueDiligenceExpiryDate { get; set; }
    
    // Partner Approval Status & Audit Trail
    // Default: NotApproved - allows editing by Partnership users
    // When Approved: only Partnership Admin can edit, audit trail captured
    public PartnerApprovalStatus PartnerApprovalStatus { get; set; } = PartnerApprovalStatus.NotApproved;
    public DateTime? PartnerApprovalDate { get; set; } // Set when approved by admin
    
    [MaxLength(500)]
    public string? PartnerApprovalReference { get; set; } // Approval notes/reference
    
    // Levy Fields
    public PartnerLevyStatus PartnerLevyStatus { get; set; }
    
    [MaxLength(500)]
    public string? ReasonForLevy { get; set; }
    
    [MaxLength(500)]
    public string? LevyTreatment { get; set; }
    
    // Operational Fields
    public bool PooledFund { get; set; } = false;
    public bool CanCreateNewOpportunities { get; set; }
    
    [MaxLength(500)]
    public string? ReasonForNoNewOpportunity { get; set; }
    

    
    // Navigation properties

    // Collection of all contacts for this partner
    public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
    
    // Computed property to get the first 5 contacts ordered by creation date (newest first)
    [NotMapped]
    public IEnumerable<Contact> First5ContactsByDate => 
        Contacts?.Where(c => c != null)
                 .OrderByDescending(c => c.CreatedDate == DateTime.MinValue ? DateTime.MinValue : c.CreatedDate)
                 .ThenByDescending(c => c.Id) // Fallback ordering by Id when CreatedDate is default
                 .Take(5) ?? Enumerable.Empty<Contact>();

    /// <summary>
    /// Gets all interactions related to this partner through its contacts
    /// </summary>
    public IEnumerable<Interaction> GetAllInteractions()
    {
        if (Contacts == null || !Contacts.Any())
            return Enumerable.Empty<Interaction>();

        return Contacts
            .Where(c => c.Interactions != null)
            .SelectMany(c => c.Interactions)
            .OrderByDescending(i => i.Date);
    }

    /// <summary>
    /// Gets recent interactions (last 10) for this partner
    /// </summary>
    public IEnumerable<Interaction> GetRecentInteractions(int count = 10)
    {
        return GetAllInteractions().Take(count);
    }

    /// <summary>
    /// Gets interactions grouped by contact for this partner
    /// </summary>
    public Dictionary<Contact, IEnumerable<Interaction>> GetInteractionsByContact()
    {
        if (Contacts == null || !Contacts.Any())
            return new Dictionary<Contact, IEnumerable<Interaction>>();

        return Contacts
            .Where(c => c.Interactions != null && c.Interactions.Any())
            .ToDictionary(
                contact => contact,
                contact => (IEnumerable<Interaction>)contact.Interactions.OrderByDescending(i => i.Date)
            );
    }

    /// <summary>
    /// Gets the count of all interactions for this partner
    /// </summary>
    public int GetTotalInteractionsCount()
    {
        return Contacts?.Sum(c => c.Interactions?.Count ?? 0) ?? 0;
    }

    /// <summary>
    /// Gets the date of the most recent interaction for this partner
    /// </summary>
    public DateTime? GetLastInteractionDate()
    {
        return GetAllInteractions().FirstOrDefault()?.Date;
    }

    /// <summary>
    /// Gets interactions by type for this partner
    /// </summary>
    public IEnumerable<Interaction> GetInteractionsByType(Domain.Enums.InteractionType type)
    {
        return GetAllInteractions().Where(i => i.Type == type);
    }

    /// <summary>
    /// Gets contact and interaction summary information
    /// </summary>
    public (int ContactsCount, int InteractionsCount, DateTime? LastInteractionDate) GetSummary()
    {
        var contactsCount = Contacts?.Count ?? 0;
        var interactionsCount = GetTotalInteractionsCount();
        var lastInteractionDate = GetLastInteractionDate();

        return (contactsCount, interactionsCount, lastInteractionDate);
    }

    
    /// <summary>
    /// Adds an organization unit relationship
    /// </summary>
    public void AddOrganizationUnitRelationship(OrganizationHierarchy organizationHierarchy)
    {
        if (organizationHierarchy == null) return;

        var relationship = new OrganizationUnitRelationship
        {
            OrganizationHierarchy = organizationHierarchy,
            EntityId = this.Id,
            EntityType = nameof(Partner),
            Name = $"Partner-{this.Id}-{organizationHierarchy.Code}",
            Status = EntityStatus.Active
        };

        OrganizationUnitRelationships.Add(relationship);
    }

    /// <summary>
    /// Removes an organization unit relationship
    /// </summary>
    public void RemoveOrganizationUnitRelationship(int organizationHierarchyId)
    {
        var relationshipsToRemove = OrganizationUnitRelationships
            .Where(r => r.OrganizationHierarchyId == organizationHierarchyId);

        foreach (var relationship in relationshipsToRemove.ToList())
        {
            OrganizationUnitRelationships.Remove(relationship);
        }
    }

    // ========== NEW WORKFLOW METHODS ==========
    
    /// <summary>
    /// Checks if partner can be activated (has required fields)
    /// </summary>
    public bool CanBeActivated()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               !string.IsNullOrWhiteSpace(PartnerShortDescription) &&
               PartnerCategoryId > 0 &&
               LiaisonOfficeId > 0;
    }
    
    /// <summary>
    /// Checks if partner is approved
    /// </summary>
    public bool IsApproved => PartnerApprovalStatus == PartnerApprovalStatus.Approved;
    
    /// <summary>
    /// Checks if due diligence is expiring (within 6 months)
    /// </summary>
    public bool IsDueDiligenceExpiring => DueDiligenceExpiryDate.HasValue && 
        DueDiligenceExpiryDate.Value.AddMonths(-6) <= DateTime.UtcNow;
        
    /// <summary>
    /// Validates minimum fields required for partner creation
    /// </summary>
    public bool HasMinimumFieldsForCreation()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
    
    /// <summary>
    /// Checks if partner has all mandatory fields filled for activation
    /// </summary>
    public bool HasMandatoryFieldsForActivation()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
    
    /// <summary>
    /// Gets validation errors for partner creation
    /// </summary>
    public List<string> GetCreationValidationErrors()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Partner Name is required");
            
        return errors;
    }
    
    /// <summary>
    /// Gets validation errors for partner activation
    /// </summary>
    public List<string> GetActivationValidationErrors()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Partner Name is required");
            
        return errors;
    }
    
    /// <summary>
    /// Gets the primary organization unit ID from the first active relationship
    /// </summary>
    public int? GetPrimaryOrgUnitId()
    {
        return OrganizationUnitRelationships
            ?.Where(r => r.Status == EntityStatus.Active && !r.IsDeleted)
            ?.FirstOrDefault()
            ?.OrganizationHierarchyId;
    }
    
    /// <summary>
    /// Gets all organization unit IDs associated with this partner
    /// </summary>
    public List<int> GetAllOrgUnitIds()
    {
        return OrganizationUnitRelationships
            ?.Where(r => r.Status == EntityStatus.Active && !r.IsDeleted)
            ?.Select(r => r.OrganizationHierarchyId)
            ?.ToList() ?? new List<int>();
    }
    

    
    /// <summary>
    /// Activates a partner if it meets the requirements
    /// </summary>
    public void ActivatePartner()
    {
        if (HasMandatoryFieldsForActivation() && Status == EntityStatus.Draft)
        {
            Status = EntityStatus.Active;
            // Partner uses consistent ID regardless of status
        }
        else
        {
            throw new InvalidOperationException("Partner cannot be activated. Check mandatory fields and status.");
        }
    }
    
    /// <summary>
    /// Approves a partner and records audit trail
    /// </summary>
    /// <param name="approverId">ID of the admin user performing the approval</param>
    /// <param name="approverName">Name of the admin user performing the approval</param>
    public void ApprovePartner(int approverId, string approverName)
    {
        if (Status != EntityStatus.Active)
        {
            throw new InvalidOperationException("Only Active partners can be approved.");
        }
        
        PartnerApprovalStatus = PartnerApprovalStatus.Approved;
        PartnerApprovalReference = $"Approved by {approverName} (ID: {approverId})";
    }
    
    /// <summary>
    /// Closes a partner
    /// </summary>
    public void ClosePartner()
    {
        if (Status == EntityStatus.Active)
        {
            Status = EntityStatus.Closed;
        }
        else
        {
            throw new InvalidOperationException("Only Active partners can be closed.");
        }
    }
    
    /// <summary>
    /// Archives a partner
    /// </summary>
    public void ArchivePartner()
    {
        if (Status == EntityStatus.Active || Status == EntityStatus.Closed)
        {
            Status = EntityStatus.Archived;
        }
        else
        {
            throw new InvalidOperationException("Only Active or Closed partners can be archived.");
        }
    }
    
    // ========== NAVIGATION PROPERTIES ==========
    public virtual LiaisonOffice LiaisonOffice { get; set; }
}

