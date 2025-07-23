namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter UNOPS partners by organizational unit hierarchy
/// </summary>
public class UNOPSPartnerByOrgUnitHierarchySpecification : BaseSpecification<UNOPSPartner>
{
    public UNOPSPartnerByOrgUnitHierarchySpecification(List<int> orgUnitHierarchyIds)
        : base(BuildCriteria(orgUnitHierarchyIds))
    {
        // Include related entities
        AddInclude(p => p.OrganizationUnitRelationships);
        AddInclude("OrganizationUnitRelationships.OrganizationHierarchy");
    }

    private static Expression<Func<UNOPSPartner, bool>> BuildCriteria(List<int> orgUnitHierarchyIds)
    {
        if (orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0)
        {
            // If no org units specified, return no results for security
            return p => false;
        }

        // Filter by any OrganizationUnitRelationships with OrganizationHierarchyId in the hierarchy
        return p => p.OrganizationUnitRelationships.Any(r => 
                   orgUnitHierarchyIds.Contains(r.OrganizationHierarchyId));
    }
}