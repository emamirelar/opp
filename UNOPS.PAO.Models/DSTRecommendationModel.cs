namespace UNOPS.PAO.Models
{
    /// <summary>
    /// Model representing a DST recommendation (AI-generated risk recommendation)
    /// Enhanced to support predefined high risks from oUP EAC checklist
    /// </summary>
    public class DSTRecommendation
    {
        /// <summary>
        /// Recommendation title
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
        /// Relevance score from vector store (0-100)
        /// For predefined risks, this equals ConfidenceLevel
        /// </summary>
        public double RelevanceScore { get; set; }

        /// <summary>
        /// Source risk ID from vector store (if available)
        /// </summary>
        public string? SourceRiskId { get; set; }

        /// <summary>
        /// oUP Question ID if this is a predefined high risk from EAC checklist
        /// Used as stable identifier for dismiss persistence and oUP mapping
        /// </summary>
        public int? OupQuestionId { get; set; }

        /// <summary>
        /// PreDefined High Risk entity ID (for linking when creating risk)
        /// </summary>
        public int? PreDefinedHighRiskId { get; set; }

        /// <summary>
        /// Risk Category ID from the predefined high risk (Level 3 category)
        /// </summary>
        public int? RiskCategoryId { get; set; }

        /// <summary>
        /// Confidence level (0-100) indicating how strongly this risk applies
        /// >= 80 means strongly recommended
        /// </summary>
        public int ConfidenceLevel { get; set; }

        /// <summary>
        /// Source type: "PREDEFINED_HIGH_RISK" or "SIMILAR_PROJECT"
        /// </summary>
        public string SourceType { get; set; } = "SIMILAR_PROJECT";

        /// <summary>
        /// Whether this is strongly recommended (confidence >= 80)
        /// </summary>
        public bool IsStronglyRecommended => ConfidenceLevel >= 80;

        /// <summary>
        /// Unique stable identifier for dismiss persistence
        /// Uses oupQuestionId for predefined risks, sourceRiskId for vector store risks
        /// Falls back to title hash for custom recommendations
        /// </summary>
        public string StableIdentifier => 
            OupQuestionId.HasValue ? $"oup_{OupQuestionId}" :
            !string.IsNullOrEmpty(SourceRiskId) ? $"vs_{SourceRiskId}" :
            $"hash_{GetTitleHash()}";

        private string GetTitleHash()
        {
            if (string.IsNullOrEmpty(Title)) return "unknown";
            var normalizedTitle = Title.ToLowerInvariant().Trim();
            if (normalizedTitle.Length > 50) normalizedTitle = normalizedTitle.Substring(0, 50);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(normalizedTitle))
                .Replace("/", "_").Replace("+", "-").Replace("=", "");
        }
    }

    /// <summary>
    /// Response model for DST recommendations endpoint
    /// </summary>
    public class DSTRecommendationsResponse
    {
        /// <summary>
        /// List of AI-generated risk recommendations
        /// </summary>
        public List<DSTRecommendation> Recommendations { get; set; } = new();

        /// <summary>
        /// Keywords extracted for semantic search
        /// </summary>
        public List<string> ExtractedKeywords { get; set; } = new();

        /// <summary>
        /// Total number of risks found from vector store
        /// </summary>
        public int TotalFound { get; set; }

        /// <summary>
        /// Execution time in milliseconds
        /// </summary>
        public long ExecutionTimeMs { get; set; }
    }

    /// <summary>
    /// Request model for DST recommendations endpoint
    /// Used to pass dismissed recommendation IDs for filtering
    /// </summary>
    public class DSTRecommendationsRequest
    {
        /// <summary>
        /// List of oupQuestionIds that the user has dismissed
        /// These will be excluded from recommendations
        /// </summary>
        public List<int> DismissedOupQuestionIds { get; set; } = new();
    }
}

