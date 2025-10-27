using UNOPS.PAO.Models.Shared;
namespace UNOPS.PAO.Models.Filters
{
    /// <summary>
    /// Request model for creating a new saved filter
    /// </summary>
    public class CreateSavedFilterRequest
    {
        /// <summary>
        /// User-friendly name for the saved filter
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional description for the filter
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// The entity type this filter applies to (e.g., "Contact", "Partner", "Interaction")
        /// </summary>
        public string EntityType { get; set; } = string.Empty;
        
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
        public string? OrderBy { get; set; }
        
        /// <summary>
        /// Ascending or descending order
        /// </summary>
        public bool? Ascending { get; set; }
    }

    /// <summary>
    /// Request model for updating an existing saved filter
    /// </summary>
    public class UpdateSavedFilterRequest
    {
        /// <summary>
        /// ID of the filter to update
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// User-friendly name for the saved filter
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional description for the filter
        /// </summary>
        public string? Description { get; set; }
        
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
        public string? OrderBy { get; set; }
        
        /// <summary>
        /// Ascending or descending order
        /// </summary>
        public bool? Ascending { get; set; }
    }

    /// <summary>
    /// Response model for saved filter information
    /// </summary>
    public class SavedFilterModel
    {
        /// <summary>
        /// Unique identifier for the saved filter
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// User-friendly name for the saved filter
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional description for the filter
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// The entity type this filter applies to
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
        public string? OrderBy { get; set; }
        
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
        
        /// <summary>
        /// When the filter was created
        /// </summary>
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// When the filter was last modified
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// Who created the filter (numeric ID)
        /// </summary>
        public int CreatedBy { get; set; }
    }

    /// <summary>
    /// Request model for retrieving saved filters with filtering options
    /// </summary>
    public class SavedFilterSearchRequest : PaginationRequest
    {
        /// <summary>
        /// Filter by entity type (optional)
        /// </summary>
        public string? EntityType { get; set; }
        
        /// <summary>
        /// Search within filter names and descriptions
        /// </summary>
        public string? SearchText { get; set; }
    }

    /// <summary>
    /// Simple model for applying saved filters in frontend
    /// </summary>
    public class ApplySavedFilterRequest
    {
        /// <summary>
        /// ID of the saved filter to apply
        /// </summary>
        public int FilterId { get; set; }
        
        /// <summary>
        /// Page index for pagination (default: 1)
        /// </summary>
        public int PageIndex { get; set; } = 1;
        
        /// <summary>
        /// Page size for pagination (default: 10)
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
} 