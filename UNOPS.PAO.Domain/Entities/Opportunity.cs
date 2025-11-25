using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UNOPS.PAO.Domain.Entities;

public class Opportunity : ModifiableDeletableEntity
{
    public new int Id { get; set; }

    public new required string Name { get; set; }
    
    public required string Description { get; set; }
    
    [MaxLength(255)]
    public string? PartnerReference { get; set; }
    
    public int? WorkflowStageId { get; set; }
    public virtual WorkflowStage? WorkflowStage { get; set; }
    
    public int? ResponsibleOrgUnitId { get; set; }
    public virtual OrganizationHierarchy? ResponsibleOrgUnit { get; set; }
    
    [MaxLength(255)]
    public string? PartnershipAgreementReference { get; set; }
    
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? InitiativeBudgetUSD { get; set; }
    
    public DateTime? TargetSigningDate { get; set; }
    
    public DateTime? TargetDeliveryDate { get; set; }
    
    public int? ProposedInitiativeTypeId { get; set; }
    public virtual ProposedInitiativeType? ProposedInitiativeType { get; set; }
    
    [MaxLength(1000)]
    public string? StrategicAlignment { get; set; }
    
    [MaxLength(2000)]
    public string? ResultsFocus { get; set; }
    
    [MaxLength(2000)]
    public string? IntendedImpactOutcomes { get; set; }
    
    [MaxLength(1000)]
    public string? ExpectedBeneficiaries { get; set; }
    
    [MaxLength(2000)]
    public string? Challenges { get; set; }
    
    /// <summary>
    /// AI-generated opportunity statement in markdown format
    /// </summary>
    public string? OpportunityStatementMarkdown { get; set; }
    
    /// <summary>
    /// Whether funding is pooled across multiple partners
    /// </summary>
    public bool IsPooledFunding { get; set; }
    
    public virtual ICollection<OpportunityFundingPartner> FundingPartners { get; set; } = new HashSet<OpportunityFundingPartner>();
    
    public virtual ICollection<OpportunityClientPartner> ClientPartners { get; set; } = new HashSet<OpportunityClientPartner>();
    
    public virtual ICollection<OpportunityStakeholder> Stakeholders { get; set; } = new HashSet<OpportunityStakeholder>();
    
    public virtual ICollection<OpportunityExternalStakeholder> ExternalStakeholders { get; set; } = new HashSet<OpportunityExternalStakeholder>();
    
    /// <summary>
    /// Free-text list of external stakeholders not found in the contact list
    /// </summary>
    [MaxLength(2000)]
    public string? MiscExternalStakeholders { get; set; }
    
    /// <summary>
    /// Additional notes about external stakeholders (e.g., their influence, capacity, role)
    /// </summary>
    [MaxLength(2000)]
    public string? ExternalStakeholderNotes { get; set; }
    
    public virtual ICollection<OpportunityDeliverable> Deliverables { get; set; } = new HashSet<OpportunityDeliverable>();
    
    public virtual ICollection<OpportunityCountry> Countries { get; set; } = new HashSet<OpportunityCountry>();
    
    public virtual ICollection<OpportunitySDG> SDGs { get; set; } = new HashSet<OpportunitySDG>();
    
    public virtual ICollection<OpportunitySDGTarget> SDGTargets { get; set; } = new HashSet<OpportunitySDGTarget>();
    
    public virtual ICollection<OpportunitySDGIndicator> SDGIndicators { get; set; } = new HashSet<OpportunitySDGIndicator>();
    
    public virtual List<Document>? Documents { get; set; }
    
    [NotMapped]
    public virtual ICollection<EntityRolePerson>? RoleAssignments { get; set; }
}

