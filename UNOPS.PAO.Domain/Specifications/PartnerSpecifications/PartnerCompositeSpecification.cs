namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// A composite specification that allows filtering partners by multiple criteria
/// </summary>
public class PartnerCompositeSpecification : GenericCompositeSpecification<Partner, IPartnerSearchFilter>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for partners
    /// </summary>
    /// <param name="filter">The filter containing all search criteria</param>
    public PartnerCompositeSpecification(IPartnerSearchFilter filter)
        : base(filter)
    {
        // Include related entities
        
        // Apply dynamic ordering based on filter properties
        ApplyDynamicOrdering(filter);
    }

    /// <summary>
    /// Applies ordering based on the filter's OrderBy and Ascending properties
    /// </summary>
    /// <param name="filter">The filter containing ordering information</param>
    private void ApplyDynamicOrdering(IPartnerSearchFilter filter)
    {
        // Get the OrderBy and Ascending values directly from the interface (type-safe)
        string? orderByField = filter.OrderBy;
        bool ascending = filter.Ascending ?? true;
        
        // Determine the ordering expression based on the field name
        Expression<Func<Partner, object>> orderExpression = GetOrderByExpression(orderByField);
        
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
    private static Expression<Func<Partner, object>> GetOrderByExpression(string? orderByField)
    {
        return orderByField?.ToLowerInvariant() switch
        {
            "name" => p => p.Name,
            "shortname" => p => p.ShortName,
            "status" => p => p.Status,
            "phone" => p => p.Phone,
            "website" => p => p.Website,
            "createddate" => p => p.CreatedDate,
            "address1city" => p => p.Address1City,
            "addresscity" => p => p.Address1City,
            "address1country" => p => p.Address1Country,
            "addresscountry" => p => p.Address1Country,
            "address1street" => p => p.Address1Street,
            "addressstreet" => p => p.Address1Street,
            _ => p => p.Name // Default to Name if no field specified or unknown field
        };
    }
} 