using System.Collections.Generic;

namespace UNOPS.PAO.Models
{
    /// <summary>
    /// Response model for global search across all entities using PostgreSQL search_entity_records function
    /// </summary>
    public class GlobalSearchResponse
    {
        public List<GlobalSearchResult> Partners { get; set; } = new List<GlobalSearchResult>();
        public List<GlobalSearchResult> Contacts { get; set; } = new List<GlobalSearchResult>();
        public List<GlobalSearchResult> Interactions { get; set; } = new List<GlobalSearchResult>();
        public string SearchQuery { get; set; } = "";
        public double ExecutionTimeMs { get; set; }
        public int TotalResults => (Partners?.Count ?? 0) + (Contacts?.Count ?? 0) + (Interactions?.Count ?? 0);
    }

    /// <summary>
    /// Individual search result item from PostgreSQL search_entity_records function
    /// </summary>
    public class GlobalSearchResult
    {
        public string EntityType { get; set; } = "";
        public int EntityId { get; set; }
        public double Score { get; set; }
        public string MatchedField { get; set; } = "";
        public string FieldValue { get; set; } = "";
        public string SearchType { get; set; } = "";
        public string MatchCriteria { get; set; } = "";
        public string Snippet { get; set; } = "";
    }
}
