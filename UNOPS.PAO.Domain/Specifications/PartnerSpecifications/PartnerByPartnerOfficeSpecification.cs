namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by organization hierarchy
/// </summary>
public class PartnerByPartnerOfficeSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by organization hierarchy ID
    /// </summary>
    /// <param name="organizationHierarchyId">The organization hierarchy ID to filter by</param>
    public PartnerByPartnerOfficeSpecification(int organizationHierarchyId)
        : base(p => p.OrganizationUnitRelationships.Any(r => r.OrganizationHierarchyId == organizationHierarchyId))
    {
        // Include related entities
        AddInclude(p => p.OrganizationUnitRelationships);
        AddInclude("OrganizationUnitRelationships.OrganizationHierarchy");
    }
} 