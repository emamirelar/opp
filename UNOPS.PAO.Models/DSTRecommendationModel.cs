namespace UNOPS.PAO.Models
{
    /// <summary>
    /// Model representing a DST recommendation (AI-generated risk recommendation)
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
        /// </summary>
        public double RelevanceScore { get; set; }

        /// <summary>
        /// Source risk ID from vector store (if available)
        /// </summary>
        public string? SourceRiskId { get; set; }
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
}

