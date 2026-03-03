namespace UNOPS.PAO.UNOPSDomain.Specifications;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// A composite specification that allows filtering UNOPS contacts by multiple criteria
/// </summary>
public class UNOPSContactCompositeSpecification : GenericCompositeSpecification<UNOPSContact, IContactSearchFilter>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for UNOPS contacts
    /// </summary>
    /// <param name="filter">The filter containing all search criteria</param>
    public UNOPSContactCompositeSpecification(IContactSearchFilter filter)
        : base(filter)
    {
        // Include related entities
        AddInclude(c => c.Partner!);
        
        // Apply dynamic ordering based on filter properties
        ApplyDynamicOrdering(filter);
    }

    /// <summary>
    /// Applies ordering based on the filter's OrderBy and Ascending properties
    /// </summary>
    /// <param name="filter">The filter containing ordering information</param>
    private void ApplyDynamicOrdering(IContactSearchFilter filter)
    {
        // Get the OrderBy and Ascending values directly from the interface (type-safe)
        string? orderByField = filter.OrderBy;
        bool ascending = filter.Ascending ?? true;
        
        // Determine the ordering expression based on the field name
        Expression<Func<UNOPSContact, object>> orderExpression = GetOrderByExpression(orderByField);
        
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
    [return: NotNull]
    private static Expression<Func<UNOPSContact, object>> GetOrderByExpression(string? orderByField)
    {
        var orderKey = orderByField?.ToLowerInvariant() ?? string.Empty;
        Expression<Func<UNOPSContact, object>> result = orderKey switch
        {
            "firstname" => c => c.FirstName ?? "",
            "lastname" => c => c.LastName ?? "",
            "email" => c => c.Email ?? "",
            "title" => c => c.Title ?? "",
            "department" => c => c.Department ?? "",
            "phone" => c => c.Phone ?? "",
            "mobile" => c => c.Mobile ?? "",
            "createddate" => c => c.CreatedDate,
            "partner" => c => (c.Partner != null ? c.Partner.Name : null) ?? "",
            "partnername" => c => (c.Partner != null ? c.Partner.Name : null) ?? "",
            _ => c => c.LastName ?? "" // Default to LastName if no field specified or unknown field
        };
        return result;
    }
}