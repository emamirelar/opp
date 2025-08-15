namespace UNOPS.PAO.UNOPSDomain.Specifications;

using System;
using System.Linq.Expressions;
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
        AddInclude(p => p.PartnerGroup);
        AddInclude(p => p.Projects);
        
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
    private static Expression<Func<UNOPSPartner, object>> GetOrderByExpression(string? orderByField)
    {
        return orderByField?.ToLowerInvariant() switch
        {
            "partnerdescription" => p => p.PartnerDescription ?? "",
            "partnershortdescription" => p => p.PartnerShortDescription ?? "",
            "partnerlongdescription" => p => p.PartnerLongDescription ?? "",
            "systemstatus" => p => p.SystemStatus,
            "createddate" => p => p.CreatedDate,
            "partnercode" => p => p.PartnerCode ?? "", // UNOPSPartner specific field
            "partnercategoryid" => p => p.PartnerCategoryId,
            "partnergroupcode" => p => p.PartnerGroupCode ?? "",
            "partnerorgunitid" => p => p.PartnerOrgUnitId ?? 0,
            "partnerinternalreportlevel" => p => p.PartnerInternalReportLevel ?? 0,
            "partnerexternalreportlevel" => p => p.PartnerExternalReportLevel ?? 0,
            "partnerlevelcode" => p => p.PartnerLevelCode ?? "",
            "partnerlevelshort" => p => p.PartnerLevelShort ?? "",
            "erpdimvalue" => p => p.ErpDimValue ?? 0,
            "partnerliaisonoffice" => p => p.PartnerLiaisonOffice ?? "",
            "unandstateentity" => p => p.UNAndStateEntity,
            "partnerscope" => p => p.PartnerScope ?? 0,
            "partnerappro​valstatus" => p => p.PartnerApprovalStatus,
            "partnerapprovaldate" => p => p.PartnerApprovalDate ?? DateTime.MinValue,
            "keyglobalpartner" => p => p.KeyGlobalPartner,
            "unsecretariatpartner" => p => p.UNSecretariatPartner,
            "duediligencerequired" => p => p.DueDiligenceRequired,
            "duediligenceapproval" => p => p.DueDiligenceApproval,
            "partnerlevystatus" => p => p.PartnerLevyStatus,
            "pooledfundnew" => p => p.PooledFund,
            "cancreatenewopportunities" => p => p.CanCreateNewOpportunities,
            _ => p => p.PartnerDescription ?? "" // Default to PartnerDescription if no field specified or unknown field
        };
    }
}