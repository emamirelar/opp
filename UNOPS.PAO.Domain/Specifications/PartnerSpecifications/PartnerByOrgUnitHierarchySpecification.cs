namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter partners by organizational unit hierarchy
/// </summary>
public class PartnerByOrgUnitHierarchySpecification : BaseSpecification<Partner>
{
    public PartnerByOrgUnitHierarchySpecification(List<int> orgUnitHierarchyIds)
        : base(BuildCriteria(orgUnitHierarchyIds))
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
    }

    private static Expression<Func<Partner, bool>> BuildCriteria(List<int> orgUnitHierarchyIds)
    {
        if (orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0)
        {
            // If no org units specified, return no results for security
            return p => false;
        }

        // Filter by any PartnerOfficeId in the hierarchy
        return p => p.PartnerOfficeId.HasValue && 
                   orgUnitHierarchyIds.Contains(p.PartnerOfficeId.Value);
    }
}