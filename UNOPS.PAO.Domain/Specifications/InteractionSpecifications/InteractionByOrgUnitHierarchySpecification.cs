namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter interactions by organizational unit hierarchy
/// </summary>
public class InteractionByOrgUnitHierarchySpecification : BaseSpecification<Interaction>
{
    public InteractionByOrgUnitHierarchySpecification(List<int> orgUnitHierarchyIds)
        : base(BuildCriteria(orgUnitHierarchyIds))
    {
        // Include related entities
        AddInclude(i => i.OrgUnit);
    }

    private static Expression<Func<Interaction, bool>> BuildCriteria(List<int> orgUnitHierarchyIds)
    {
        if (orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0)
        {
            // If no org units specified, return no results for security
            return i => false;
        }

        // Filter by OrgUnitId in the hierarchy
        return i => i.OrgUnitId.HasValue && 
                   orgUnitHierarchyIds.Contains(i.OrgUnitId.Value);
    }
}