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

    /// <summary>
    /// Creates a specification for advanced search on contacts
    /// </summary>
    /// <param name="filter">The filter containing advanced search criteria</param>
    public ContactCompositeSpecification(IContactSearchFilter filter)
        : base(filter)
    {
        // Include the related partner
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
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    private static Expression<Func<Contact, object>> GetOrderByExpression(string? orderByField)
    {
        var orderKey = orderByField?.ToLowerInvariant() ?? string.Empty;
        Expression<Func<Contact, object>> result = orderKey switch
        {
            "firstname" => c => c.FirstName ?? "",
            "lastname" => c => c.LastName ?? "",
            "email" => c => c.Email ?? "",
            "title" => c => c.Title ?? "",
            "department" => c => c.Department ?? "",
            "phone" => c => c.Phone ?? "",
            "mobile" => c => c.Mobile ?? "",
            "createddate" => c => c.CreatedDate,
            "partner" => c => (object)(c.Partner != null ? c.Partner.Name ?? "" : ""),
            "partnername" => c => (object)(c.Partner != null ? c.Partner.Name ?? "" : ""),
            _ => c => c.LastName ?? "" // Default to LastName if no field specified or unknown field
        };
        return result!;
    }
} 