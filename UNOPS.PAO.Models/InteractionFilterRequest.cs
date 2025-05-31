// Define the request model that extends PaginationRequest
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Specifications.Interfaces;

namespace UNOPS.PAO.Models;

public class InteractionFilterRequest : PaginationRequest, IInteractionSearchFilter
{
    public int? Id { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public InteractionType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public DateTime? Date { get; set; }
    public string? Description { get; set; }
    public string? Subject { get; set; }
    public string? SearchText { get; set; }
    
    // Advanced search properties
    public bool AdvancedSearch { get; set; }
    public string? SearchCriteria { get; set; }
    
    // Computed property for Type string (for interface compatibility)
    string? IInteractionSearchFilter.Type 
    { 
        get => Type?.ToString(); 
        set => Type = Enum.TryParse<InteractionType>(value, out var result) ? result : null; 
    }
}
