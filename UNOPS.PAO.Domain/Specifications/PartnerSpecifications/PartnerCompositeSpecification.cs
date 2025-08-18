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
    /// Updated to use new enhanced Partner field structure
    /// </summary>
    /// <param name="orderByField">The field name to order by</param>
    /// <returns>The ordering expression</returns>
    private static Expression<Func<Partner, object>> GetOrderByExpression(string? orderByField)
    {
        return orderByField?.ToLowerInvariant() switch
        {
            "name" => p => p.PartnerDescription ?? "",
            "partnerdescription" => p => p.PartnerDescription ?? "",
            "shortname" => p => p.PartnerShortDescription ?? "",
            "partnershortdescription" => p => p.PartnerShortDescription ?? "",
            "status" => p => p.Status,
            "systemstatus" => p => p.Status,
            "partnerstatus" => p => p.Status,
            "phone" => p => p.PartnerDescription ?? "", // Phone field deprecated, fallback to description
            "website" => p => p.PartnerDescription ?? "", // Website field deprecated, fallback to description
            "createddate" => p => p.CreatedDate,
            "address1city" => p => p.PartnerDescription ?? "", // Address fields deprecated, fallback to description
            "addresscity" => p => p.PartnerDescription ?? "",
            "address1country" => p => p.PartnerDescription ?? "",
            "addresscountry" => p => p.PartnerDescription ?? "",
            "address1street" => p => p.PartnerDescription ?? "",
            "addressstreet" => p => p.PartnerDescription ?? "",
            "partnercategoryid" => p => p.PartnerCategoryId,
            "partnergroupcode" => p => p.PartnerGroupCode ?? "",
            "approvalstatus" => p => p.PartnerApprovalStatus,
            _ => p => p.PartnerDescription ?? "" // Default to PartnerDescription if no field specified or unknown field
        };
    }
} 