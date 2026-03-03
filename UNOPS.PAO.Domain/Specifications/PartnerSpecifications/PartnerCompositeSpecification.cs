namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using System;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
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
        var orderKey = orderByField?.ToLowerInvariant() ?? string.Empty;
        Expression<Func<Partner, object>> result = orderKey switch
        {
            "name" => (Expression<Func<Partner, object>>)(p => p.Name ?? ""),
            "partnershortdescription" => (Expression<Func<Partner, object>>)(p => (object)(p.PartnerShortDescription ?? "")),
            "partnerlongdescription" => (Expression<Func<Partner, object>>)(p => (object)(p.PartnerLongDescription ?? "")),
            "status" => (Expression<Func<Partner, object>>)(p => p.Status),
            "createddate" => (Expression<Func<Partner, object>>)(p => p.CreatedDate),
            "lastmodifieddate" => (Expression<Func<Partner, object>>)(p => p.LastModifiedDate!),
            "partnercategoryid" => (Expression<Func<Partner, object>>)(p => p.PartnerCategoryId!),
            "partnergroupid" => (Expression<Func<Partner, object>>)(p => p.PartnerGroupId ?? 0),
            "partnerapprovalstatus" => (Expression<Func<Partner, object>>)(p => p.PartnerApprovalStatus),
            "keyglobalpartner" => (Expression<Func<Partner, object>>)(p => p.KeyGlobalPartner),
            "unsecretariatpartner" => (Expression<Func<Partner, object>>)(p => p.UNSecretariatPartner),
            "unandstateentity" => (Expression<Func<Partner, object>>)(p => p.UNAndStateEntity),
            "pooledfund" => (Expression<Func<Partner, object>>)(p => p.PooledFund),
            "cancreatenewopportunities" => (Expression<Func<Partner, object>>)(p => p.CanCreateNewOpportunities),
            "liaisonofficeid" => (Expression<Func<Partner, object>>)(p => p.LiaisonOfficeId ?? 0),
            "partnerfocalpointuserid" => (Expression<Func<Partner, object>>)(p => p.PartnerFocalPointUserId ?? 0),
            "erpdimvalue" => (Expression<Func<Partner, object>>)(p => p.ErpDimValue ?? 0),
            "partnerlevystatus" => (Expression<Func<Partner, object>>)(p => (object)((int)(p.PartnerLevyStatus ?? default(PartnerLevyStatus)))),
            "duediligencerequired" => (Expression<Func<Partner, object>>)(p => p.DueDiligenceRequired!),
            "duediligenceapproval" => (Expression<Func<Partner, object>>)(p => (object)(p.DueDiligenceApproval ?? default(DueDiligenceApproval))),
            "duediligenceapprovaldate" => (Expression<Func<Partner, object>>)(p => p.DueDiligenceApprovalDate ?? DateTime.MinValue),
            "duediligenceexpirydate" => (Expression<Func<Partner, object>>)(p => p.DueDiligenceExpiryDate ?? DateTime.MinValue),
            "partnerapprovaldate" => (Expression<Func<Partner, object>>)(p => p.PartnerApprovalDate ?? DateTime.MinValue),
            _ => (Expression<Func<Partner, object>>)(p => p.Name ?? "") // Default to Name if no field specified or unknown field
        };
        return result!;
    }
} 