// Define the request model that extends PaginationRequest
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Models.Interactions;

public class InteractionFilterRequest : PaginationRequest, IInteractionSearchFilter, IValidatableObject
{
    public int? Id { get; set; }
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public int? PartnerId { get; set; }
    public InteractionType? Type { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public DateOnly? Date { get; set; }
    public string? Description { get; set; }
    public string? Subject { get; set; }
    public string? SearchText { get; set; }
    
    // Organization Unit filter - filters results by organizational unit (includes hierarchy)
    public int? OrgUnitId { get; set; }
    
    // Advanced search properties
    public bool AdvancedSearch { get; set; }
    public string? SearchCriteria { get; set; }
    
    // Explicit interface implementations for date conversions
    DateTime? IInteractionSearchFilter.FromDate 
    { 
        get => FromDate?.ToDateTime(TimeOnly.MinValue); 
        set => FromDate = value?.Date != null ? DateOnly.FromDateTime(value.Value) : null; 
    }
    
    DateTime? IInteractionSearchFilter.ToDate 
    { 
        get => ToDate?.ToDateTime(TimeOnly.MinValue); 
        set => ToDate = value?.Date != null ? DateOnly.FromDateTime(value.Value) : null; 
    }
    
    DateTime? IInteractionSearchFilter.Date 
    { 
        get => Date?.ToDateTime(TimeOnly.MinValue); 
        set => Date = value?.Date != null ? DateOnly.FromDateTime(value.Value) : null; 
    }
    
    // Computed property for Type string (for interface compatibility)
    string? IInteractionSearchFilter.Type 
    { 
        get => Type?.ToString(); 
        set => Type = Enum.TryParse<InteractionType>(value, out var result) ? result : null; 
    }

    /// <summary>
    /// Custom validation logic for the interaction filter request
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // Validate date ranges
        if (FromDate.HasValue && ToDate.HasValue && FromDate > ToDate)
        {
            results.Add(new ValidationResult(
                "FromDate cannot be later than ToDate", 
                new[] { nameof(FromDate), nameof(ToDate) }));
        }

        // Validate date not in future for specific dates
        if (Date.HasValue && Date > DateOnly.FromDateTime(DateTime.Today))
        {
            results.Add(new ValidationResult(
                "Date cannot be in the future", 
                new[] { nameof(Date) }));
        }

        // Validate FromDate not too far in the past (e.g., more than 10 years)
        var tenYearsAgo = DateOnly.FromDateTime(DateTime.Today.AddYears(-10));
        if (FromDate.HasValue && FromDate < tenYearsAgo)
        {
            results.Add(new ValidationResult(
                "FromDate cannot be more than 10 years in the past", 
                new[] { nameof(FromDate) }));
        }

        // Validate advanced search criteria
        if (AdvancedSearch && string.IsNullOrWhiteSpace(SearchCriteria))
        {
            results.Add(new ValidationResult(
                "SearchCriteria is required when AdvancedSearch is enabled", 
                new[] { nameof(SearchCriteria) }));
        }

        // Validate IDs are positive
        if (Id.HasValue && Id <= 0)
        {
            results.Add(new ValidationResult(
                "Id must be a positive number", 
                new[] { nameof(Id) }));
        }

        if (ContactId.HasValue && ContactId <= 0)
        {
            results.Add(new ValidationResult(
                "ContactId must be a positive number", 
                new[] { nameof(ContactId) }));
        }

        if (PartnerId.HasValue && PartnerId <= 0)
        {
            results.Add(new ValidationResult(
                "PartnerId must be a positive number", 
                new[] { nameof(PartnerId) }));
        }

        return results;
    }
}
