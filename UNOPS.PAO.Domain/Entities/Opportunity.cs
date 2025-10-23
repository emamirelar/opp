using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class Opportunity : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    /// <summary>
    /// Name of the opportunity (inherited from ModifiableDeletableEntity, but made required)
    /// </summary>
    public new required string Name { get; set; }
    
    /// <summary>
    /// Description/brief narrative summary of the initiative (required)
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Partner reference ID or short name by which the initiative is referred to by the partner
    /// </summary>
    [MaxLength(255)]
    public string? PartnerReference { get; set; }
    
    // Status inherited from ModifiableDeletableEntity (Draft, Active, OnHold, Closed, Archived)
    
    /// <summary>
    /// Current workflow stage of the opportunity
    /// </summary>
    public int? WorkflowStageId { get; set; }
    public virtual WorkflowStage? WorkflowStage { get; set; }
    
    /// <summary>
    /// Organizational unit responsible for developing the opportunity
    /// </summary>
    public int? ResponsibleOrgUnitId { get; set; }
    public virtual OrganizationHierarchy? ResponsibleOrgUnit { get; set; }
    
    /// <summary>
    /// Partnership Agreement Reference (if applicable)
    /// </summary>
    [MaxLength(255)]
    public string? PartnershipAgreementReference { get; set; }
    
    /// <summary>
    /// Estimated total budget required for the initiative (USD)
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? InitiativeBudgetUSD { get; set; }
    
    /// <summary>
    /// Target date for signing the opportunity agreement
    /// </summary>
    public DateTime? TargetSigningDate { get; set; }
    
    /// <summary>
    /// Target date for delivery completion
    /// </summary>
    public DateTime? TargetDeliveryDate { get; set; }
    
    /// <summary>
    /// Proposed initiative type to be developed (post Go decision)
    /// </summary>
    public int? ProposedInitiativeTypeId { get; set; }
    public virtual ProposedInitiativeType? ProposedInitiativeType { get; set; }
    
    // Navigation properties for child entities
    
    /// <summary>
    /// Funding partners contributing to the opportunity
    /// </summary>
    public virtual ICollection<OpportunityFundingPartner> FundingPartners { get; set; } = new HashSet<OpportunityFundingPartner>();
    
    /// <summary>
    /// Client partners (beneficiary organizations)
    /// </summary>
    public virtual ICollection<OpportunityClientPartner> ClientPartners { get; set; } = new HashSet<OpportunityClientPartner>();
    
    /// <summary>
    /// Internal and external stakeholders
    /// </summary>
    public virtual ICollection<OpportunityStakeholder> Stakeholders { get; set; } = new HashSet<OpportunityStakeholder>();
    
    /// <summary>
    /// Deliverables for the opportunity
    /// </summary>
    public virtual ICollection<OpportunityDeliverable> Deliverables { get; set; } = new HashSet<OpportunityDeliverable>();
    
    /// <summary>
    /// Countries of implementation
    /// </summary>
    public virtual ICollection<OpportunityCountry> Countries { get; set; } = new HashSet<OpportunityCountry>();
    
    /// <summary>
    /// Linked SDGs (Sustainable Development Goals)
    /// </summary>
    public virtual ICollection<OpportunitySDG> SDGs { get; set; } = new HashSet<OpportunitySDG>();
    
    /// <summary>
    /// Documents associated with the opportunity
    /// </summary>
    public virtual List<Document>? Documents { get; set; }
    
    /// <summary>
    /// Entity role assignments (e.g., Opportunity Manager)
    /// </summary>
    [NotMapped] // Will be queried from EntityRolePerson table
    public virtual ICollection<EntityRolePerson>? RoleAssignments { get; set; }
}

