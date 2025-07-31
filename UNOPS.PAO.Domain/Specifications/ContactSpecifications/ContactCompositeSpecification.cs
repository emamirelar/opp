namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// A specification for advanced search on contacts using search criteria
/// </summary>
public class ContactCompositeSpecification : GenericCompositeSpecification<Contact, IContactSearchFilter>
{
    private bool nullableBool;

    /// <summary>
    /// Creates a specification for advanced search on contacts
    /// </summary>
    /// <param name="filter">The filter containing advanced search criteria</param>
    public ContactCompositeSpecification(IContactSearchFilter filter)
        : base(filter)
    {
        // Include the related partner
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
        Expression<Func<Contact, object>> orderExpression = GetOrderByExpression(orderByField);
        
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
    private static Expression<Func<Contact, object>> GetOrderByExpression(string? orderByField)
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