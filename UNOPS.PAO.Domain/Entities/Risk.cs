using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities
{
    /// <summary>
    /// Represents a risk in the risk register associated with an entity (Opportunity, Project, etc.)
    /// </summary>
    public class Risk : ModifiableDeletableEntity<int, int>
    {
        /// <summary>
        /// The type of entity this risk is associated with (e.g., "Opportunity", "Project")
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the entity this risk is associated with
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Risk title - concise summary of the risk
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the risk
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Recommendation for mitigating or managing the risk
        /// </summary>
        public string Recommendation { get; set; } = string.Empty;

        /// <summary>
        /// Impact level of the risk (Low, Medium, High)
        /// </summary>
        public RiskImpact Impact { get; set; } = RiskImpact.Medium;

        /// <summary>
        /// Current status of the risk (Open, Mitigated, Accepted, Closed)
        /// </summary>
        public RiskStatus RiskStatus { get; set; } = RiskStatus.Open;

        /// <summary>
        /// Date when the risk was identified
        /// </summary>
        public DateTime? IdentifiedDate { get; set; }

        /// <summary>
        /// User ID of the person who identified the risk
        /// </summary>
        public int? IdentifiedBy { get; set; }
    }
}

