namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using System;
using System.Linq.Expressions;
using System.Text;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// A specification for advanced search on interactions using search criteria
/// </summary>
public class InteractionCompositeSpecification : GenericCompositeSpecification<Interaction, IInteractionSearchFilter>
{
    /// <summary>
    /// Creates a specification for advanced search on interactions
    /// </summary>
    /// <param name="filter">The filter containing advanced search criteria</param>
    public InteractionCompositeSpecification(IInteractionSearchFilter filter)
        : base(filter)
    {
        // Include the related contacts through junction table
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
        
        // Include the related partners through junction table
        AddInclude(i => i.InteractionPartners!);
        AddInclude("InteractionPartners.Partner");
        
        // Apply dynamic ordering based on filter properties
        ApplyDynamicOrdering(filter);
    }

    /// <summary>
    /// Creates a composite specification with multiple filter criteria for interactions (legacy constructor for backward compatibility)
    /// </summary>
    /// <param name="contactId">Optional contact ID to filter by</param>
    /// <param name="type">Optional interaction type to filter by</param>
    /// <param name="fromDate">Optional start date to filter by</param>
    /// <param name="toDate">Optional end date to filter by</param>
    /// <param name="searchText">Optional text to search for in interaction description</param>
    [Obsolete("Use the constructor with IInteractionSearchFilter instead")]
    public InteractionCompositeSpecification(
        int? contactId = null,
        InteractionType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchText = null)
        : base(CreateLegacyFilter(contactId, type, fromDate, toDate, searchText))
    {
        // Include the related contacts through junction table
        AddInclude(i => i.InteractionContacts!);
        AddInclude("InteractionContacts.Contact");
        
        // Include the related partners through junction table
        AddInclude(i => i.InteractionPartners!);
        AddInclude("InteractionPartners.Partner");
        
        // Default ordering is by date descending
        ApplyOrderByDescending(i => i.Date);
    }

    /// <summary>
    /// Creates a legacy filter for backward compatibility
    /// </summary>
    private static IInteractionSearchFilter CreateLegacyFilter(
        int? contactId,
        InteractionType? type,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchText)
    {
        return new LegacyInteractionSearchFilter
        {
            ContactId = contactId,
            Type = type?.ToString(),
            FromDate = fromDate,
            ToDate = toDate,
            SearchText = searchText,
            AdvancedSearch = false,
            SearchCriteria = null
        };
    }

    /// <summary>
    /// Legacy filter implementation for backward compatibility
    /// </summary>
    private class LegacyInteractionSearchFilter : IInteractionSearchFilter
    {
        public int? Id { get; set; }
        public int? ContactId { get; set; }
        public string? ContactName { get; set; }
        public int? PartnerId { get; set; }
        public string? Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
        public string? Subject { get; set; }
        
        public string? SearchText { get; set; }
        public bool AdvancedSearch { get; set; }
        public string? SearchCriteria { get; set; }
        public int? OrgUnitId { get; set; }
        
        // IPaginationFilter properties
        public string? OrderBy { get; set; }
        public bool? Ascending { get; set; }
    }

    /// <summary>
    /// Applies ordering based on the filter's OrderBy and Ascending properties
    /// </summary>
    /// <param name="filter">The filter containing ordering information</param>
    private void ApplyDynamicOrdering(IInteractionSearchFilter filter)
    {
        // Get the OrderBy and Ascending values directly from the interface (type-safe)
        string? orderByField = filter.OrderBy;
        bool ascending = filter.Ascending ?? true;
        
        // Determine the ordering expression based on the field name
        Expression<Func<Interaction, object>> orderExpression = GetOrderByExpression(orderByField);
        
        // Apply the correct ordering method
        if (ascending)
        {
            ApplyOrderBy(orderExpression);
        }
        else
        {
            ApplyOrderByDescending(orderExpression);
        }
    }

    /// <summary>
    /// Gets the appropriate ordering expression for the specified field
    /// </summary>
    /// <param name="orderByField">The field name to order by</param>
    /// <returns>The ordering expression</returns>
    private static Expression<Func<Interaction, object>> GetOrderByExpression(string? orderByField)
    {
        var orderKey = orderByField?.ToLowerInvariant() ?? string.Empty;
        return orderKey switch
        {
            "date" => i => i.Date,
            "subject" => i => i.Subject ?? string.Empty,
            "description" => i => i.Description ?? string.Empty,
            "type" => i => i.Type,
            "createddate" => i => i.CreatedDate,
            _ => i => i.Date // Default to Date descending (most recent first) if no field specified or unknown field
        };
    }
} 