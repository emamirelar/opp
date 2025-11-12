namespace UNOPS.PAO.Models
{
    /// <summary>
    /// Model representing a risk in the risk register
    /// </summary>
    public class RiskModel
    {
        /// <summary>
        /// Risk ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The type of entity this risk is associated with (e.g., "Opportunity", "Project")
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the entity this risk is associated with
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Risk title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the risk
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Recommendation for mitigating the risk
        /// </summary>
        public string Recommendation { get; set; } = string.Empty;

        /// <summary>
        /// Impact level: 1=Low, 2=Medium, 3=High
        /// </summary>
        public int Impact { get; set; }

        /// <summary>
        /// Current status of the risk
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Date when the risk was identified
        /// </summary>
        public DateTime? IdentifiedDate { get; set; }

        /// <summary>
        /// Name of the person who identified the risk
        /// </summary>
        public string? IdentifiedBy { get; set; }

        /// <summary>
        /// Date when the risk was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Name of the person who created the risk record
        /// </summary>
        public string? CreatedBy { get; set; }
    }

    /// <summary>
    /// Request model for creating a new risk
    /// </summary>
    public class RiskCreateRequest
    {
        /// <summary>
        /// The ID of the entity this risk is associated with
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Risk title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the risk
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Recommendation for mitigating the risk
        /// </summary>
        public string Recommendation { get; set; } = string.Empty;

        /// <summary>
        /// Impact level: 1=Low, 2=Medium, 3=High
        /// </summary>
        public int Impact { get; set; } = 2; // Default to Medium
    }

    /// <summary>
    /// Response model for DST risks endpoint
    /// </summary>
    public class DSTRisksResponse
    {
        /// <summary>
        /// List of risks
        /// </summary>
        public List<RiskModel> Risks { get; set; } = new();

        /// <summary>
        /// Total count of risks
        /// </summary>
        public int TotalCount { get; set; }
    }
}

