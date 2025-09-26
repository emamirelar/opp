using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.Domain.Entities
{
    /// <summary>
    /// Represents a saved search filter for a user and entity type
    /// </summary>
    public class SavedFilter : ModifiableEntity
    {
        /// <summary>
        /// User-friendly name for the saved filter
        /// </summary>
        public new string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional description for the filter
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// The entity type this filter applies to (e.g., "Contact", "Partner", "Interaction")
        /// </summary>
        public string EntityType { get; set; } = string.Empty;
        
        /// <summary>
        /// The user ID who owns this filter
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// JSON string containing the search criteria
        /// </summary>
        public string SearchCriteria { get; set; } = string.Empty;
        
        /// <summary>
        /// Simple search text (if any)
        /// </summary>
        public string? SearchText { get; set; }
        
        /// <summary>
        /// Whether this is an advanced search filter
        /// </summary>
        public bool IsAdvancedSearch { get; set; }
        
        /// <summary>
        /// Ordering field for the filter
        /// </summary>
        public string? OrderByField { get; set; }
        
        /// <summary>
        /// Ascending or descending order
        /// </summary>
        public bool? Ascending { get; set; }
        
        /// <summary>
        /// Number of times this filter has been used
        /// </summary>
        public int UsageCount { get; set; }
        
        /// <summary>
        /// Last time this filter was used
        /// </summary>
        public DateTime? LastUsedDate { get; set; }
    }
} 