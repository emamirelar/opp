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
    [return: System.Diagnostics.CodeAnalysis.NotNull]
    private static Expression<Func<Partner, object>> GetOrderByExpression(string? orderByField)
    {
        return orderByField?.ToLowerInvariant() switch
        {
            "name" => p => p.Name ?? "",
            "partnershortdescription" => p => p.PartnerShortDescription ?? "",
            "partnerlongdescription" => p => p.PartnerLongDescription ?? "",
            "status" => p => p.Status,
            "createddate" => p => p.CreatedDate,
            "lastmodifieddate" => p => p.LastModifiedDate,
            "partnercategoryid" => p => p.PartnerCategoryId,
            "partnergroupid" => p => p.PartnerGroupId ?? 0,
            "partnerApprovalstatus" => p => p.PartnerApprovalStatus,
            "keyglobalpartner" => p => p.KeyGlobalPartner,
            "unsecretariatpartner" => p => p.UNSecretariatPartner,
            "unandstateentity" => p => p.UNAndStateEntity,
            "pooledFund" => p => p.PooledFund,
            "cancreatenewopportunities" => p => p.CanCreateNewOpportunities,
            "liaisonOfficeid" => p => p.LiaisonOfficeId,
            "partnerFocalPointuserid" => p => p.PartnerFocalPointUserId,
            "erpdimValue" => p => p.ErpDimValue,
            "partnerlevystatus" => p => p.PartnerLevyStatus,
            "duediligencerequired" => p => p.DueDiligenceRequired,
            "duediligenceapproval" => p => p.DueDiligenceApproval,
            "duediligenceapprovaldate" => p => p.DueDiligenceApprovalDate,
            "duediligenceexpirydate" => p => p.DueDiligenceExpiryDate,
            "partnerapprovaldate" => p => p.PartnerApprovalDate,
            _ => p => p.Name ?? "" // Default to Name if no field specified or unknown field
        };
    }
} 