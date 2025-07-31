namespace UNOPS.PAO.UNOPSDomain.Specifications;

using System;
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
        AddInclude(c => c.Partner);
        
        // Apply dynamic ordering based on filter properties
        ApplyDynamicOrdering(filter);
    }

    /// <summary>
    /// Applies ordering based on the filter's OrderBy and Ascending properties
    /// </summary>
    /// <param name="filter">The filter containing ordering information</param>
    private void ApplyDynamicOrdering(IContactSearchFilter filter)
    {
        // Get the OrderBy and Ascending values from the filter
        string? orderByField = null;
        bool ascending = true;
        
        // Check if the filter has OrderBy and Ascending properties (from PaginationRequest)
        var orderByProperty = filter.GetType().GetProperty("OrderBy");
        var ascendingProperty = filter.GetType().GetProperty("Ascending");
        
        if (orderByProperty != null)
        {
            orderByField = orderByProperty.GetValue(filter) as string;
        }
        
        if (ascendingProperty != null)
        {
            var ascendingValue = ascendingProperty.GetValue(filter) ?? true;
            if (ascendingValue is bool boolValue)
            {
                ascending = boolValue;
            }
        }
        
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
    private static Expression<Func<UNOPSContact, object>> GetOrderByExpression(string? orderByField)
    {
        return orderByField?.ToLowerInvariant() switch
        {
            "firstname" => c => c.FirstName,
            "lastname" => c => c.LastName,
            "email" => c => c.Email,
            "title" => c => c.Title,
            "department" => c => c.Department,
            "phone" => c => c.Phone,
            "mobile" => c => c.Mobile,
            "createddate" => c => c.CreatedDate,
            "partner" => c => c.Partner.Name,
            "partnername" => c => c.Partner.Name,
            _ => c => c.LastName // Default to LastName if no field specified or unknown field
        };
    }
}