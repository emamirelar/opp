using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities
{
    /// <summary>
    /// Represents a risk in the risk register associated with an entity (Opportunity, Project, etc.)
    /// Aligned with oUP risk structure for future integration
    /// </summary>
    public class Risk : ModifiableDeletableEntity<int, int>
    {
        #region Entity Association

        /// <summary>
        /// The type of entity this risk is associated with (e.g., "Opportunity", "Project")
        /// </summary>
        [MaxLength(50)]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the entity this risk is associated with
        /// </summary>
        public int EntityId { get; set; }

        #endregion

        #region Mandatory Fields (oUP aligned)

        /// <summary>
        /// Risk title - concise summary of the risk (MANDATORY)
        /// </summary>
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// FK to RiskType (Threat or Opportunity) (MANDATORY)
        /// </summary>
        public int RiskTypeId { get; set; }

        /// <summary>
        /// Navigation property to RiskType
        /// </summary>
        [ForeignKey(nameof(RiskTypeId))]
        public virtual RiskType? RiskTypeEntity { get; set; }

        /// <summary>
        /// FK to RiskCategory (Level 3 - leaf category) (MANDATORY)
        /// </summary>
        public int RiskCategoryId { get; set; }

        /// <summary>
        /// Navigation property to RiskCategory
        /// </summary>
        [ForeignKey(nameof(RiskCategoryId))]
        public virtual RiskCategory? RiskCategory { get; set; }

        /// <summary>
        /// FK to RiskProbability (MANDATORY)
        /// </summary>
        public int RiskProbabilityId { get; set; }

        /// <summary>
        /// Navigation property to RiskProbability
        /// </summary>
        [ForeignKey(nameof(RiskProbabilityId))]
        public virtual RiskProbability? RiskProbabilityEntity { get; set; }

        /// <summary>
        /// FK to RiskProximity (MANDATORY)
        /// </summary>
        public int RiskProximityId { get; set; }

        /// <summary>
        /// Navigation property to RiskProximity
        /// </summary>
        [ForeignKey(nameof(RiskProximityId))]
        public virtual RiskProximity? RiskProximityEntity { get; set; }

        /// <summary>
        /// FK to RiskImpactLevel (MANDATORY)
        /// </summary>
        public int RiskImpactLevelId { get; set; }

        /// <summary>
        /// Navigation property to RiskImpactLevel
        /// </summary>
        [ForeignKey(nameof(RiskImpactLevelId))]
        public virtual RiskImpactLevel? RiskImpactLevelEntity { get; set; }

        /// <summary>
        /// FK to RiskResponseType (CONDITIONAL - mandatory for Opportunity type)
        /// </summary>
        public int? RiskResponseTypeId { get; set; }

        /// <summary>
        /// Navigation property to RiskResponseType
        /// </summary>
        [ForeignKey(nameof(RiskResponseTypeId))]
        public virtual RiskResponseType? RiskResponseTypeEntity { get; set; }

        #endregion

        #region Optional Fields

        /// <summary>
        /// Detailed description of the risk (OPTIONAL)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Recommendation for mitigating or managing the risk (OPTIONAL)
        /// Maps to oUP response_summary
        /// </summary>
        public string Recommendation { get; set; } = string.Empty;

        #endregion

        #region Legacy Fields (for backward compatibility)

        /// <summary>
        /// Impact level of the risk (Low, Medium, High) - DEPRECATED
        /// Use RiskImpactLevelId instead for oUP alignment
        /// Kept for backward compatibility with existing code
        /// </summary>
        public RiskImpact Impact { get; set; } = RiskImpact.Medium;

        /// <summary>
        /// Current status of the risk (Open, Mitigated, Accepted, Closed)
        /// Uses RiskStatus enum for detailed workflow states
        /// </summary>
        public RiskStatus RiskStatus { get; set; } = RiskStatus.Open;

        #endregion

        #region Audit Fields

        /// <summary>
        /// Date when the risk was identified
        /// </summary>
        public DateTime? IdentifiedDate { get; set; }

        /// <summary>
        /// User ID of the person who identified the risk
        /// </summary>
        public int? IdentifiedBy { get; set; }

        #endregion

        #region PreDefined High Risk Reference

        /// <summary>
        /// FK to PreDefinedHighRisk (when created from High Risk Checklist)
        /// Null if risk was created manually
        /// </summary>
        public int? PreDefinedHighRiskId { get; set; }

        /// <summary>
        /// Navigation property to PreDefinedHighRisk
        /// </summary>
        [ForeignKey(nameof(PreDefinedHighRiskId))]
        public virtual PreDefinedHighRisk? PreDefinedHighRisk { get; set; }

        #endregion
    }
}

