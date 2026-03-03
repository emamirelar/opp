namespace UNOPS.PAO.UNOPSDomain.Specifications;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// A composite specification that allows filtering UNOPS partners by multiple criteria
/// </summary>
public class UNOPSPartnerCompositeSpecification : GenericCompositeSpecification<UNOPSPartner, IPartnerSearchFilter>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for UNOPS partners
    /// </summary>
    /// <param name="filter">The filter containing all search criteria</param>
    public UNOPSPartnerCompositeSpecification(IPartnerSearchFilter filter)
        : base(filter)
    {
        // Include related entities
        AddInclude(p => p.PartnerGroup!);
        AddInclude(p => p.LiaisonOffice!);
        
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
        Expression<Func<UNOPSPartner, object>> orderExpression = GetOrderByExpression(orderByField);
        
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
    /// Updated to use enhanced Partner field structure inherited from base Partner entity
    /// </summary>
    /// <param name="orderByField">The field name to order by</param>
    /// <returns>The ordering expression</returns>
    [return: NotNull]
    private static Expression<Func<UNOPSPartner, object>> GetOrderByExpression(string? orderByField)
    {
        var orderKey = orderByField?.ToLowerInvariant() ?? string.Empty;
        Expression<Func<UNOPSPartner, object>> result = orderKey switch
        {
            "partnerdescription" => p => p.Name ?? "",
            "partnershortdescription" => p => p.PartnerShortDescription ?? "",
            "partnerlongdescription" => p => p.PartnerLongDescription ?? "",
            "systemstatus" => p => p.Status,
            "createddate" => p => p.CreatedDate,
            "partnercategoryid" => p => p.PartnerCategoryId ?? 0,
            "partnergroupid" => p => p.PartnerGroupId ?? 0,
            "erpdimvalue" => p => p.ErpDimValue ?? 0,
            "partnerliaisonoffice" => p => (p.LiaisonOffice != null ? p.LiaisonOffice.Name : null) ?? "",
            "unandstateentity" => p => p.UNAndStateEntity,
            "partnerapprovalstatus" => p => p.PartnerApprovalStatus,
            "partnerapprovaldate" => p => p.PartnerApprovalDate ?? DateTime.MinValue,
            "keyglobalpartner" => p => p.KeyGlobalPartner,
            "unsecretariatpartner" => p => p.UNSecretariatPartner,
            "duediligencerequired" => p => (object)(p.DueDiligenceRequired ?? default(DueDiligenceRequired)),
            "duediligenceapproval" => p => (object)(p.DueDiligenceApproval ?? default(DueDiligenceApproval)),
            "partnerlevystatus" => p => (object)(p.PartnerLevyStatus ?? default(PartnerLevyStatus)),
            "pooledfundnew" => p => p.PooledFund,
            "cancreatenewopportunities" => p => p.CanCreateNewOpportunities,
            _ => p => p.Name ?? "" // Default to PartnerDescription if no field specified or unknown field
        };
        return result;
    }
}