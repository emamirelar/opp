namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Specification to filter partners by organizational unit hierarchy
/// Uses manual joins to efficiently filter at the database level without navigation properties
/// </summary>
public class PartnerByOrgUnitHierarchySpecification : BaseSpecification<Partner>
{
    private readonly List<int> _orgUnitHierarchyIds;
    
    public PartnerByOrgUnitHierarchySpecification(List<int> orgUnitHierarchyIds)
        : base(BuildCriteria(orgUnitHierarchyIds))
    {
        _orgUnitHierarchyIds = orgUnitHierarchyIds ?? new List<int>();
        // Include related entities
    }

    private static Expression<Func<Partner, bool>> BuildCriteria(List<int> orgUnitHierarchyIds)
    {
        if (orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0)
        {
            // If no org units specified, return no results for security
            return p => false;
        }

        // Basic filtering - actual org unit filtering will be done via manual join
        return p => true;
    }
    
    /// <summary>
    /// Apply manual join filtering to the query for efficient database-level filtering
    /// This should be called by the repository/manager when applying the specification
    /// </summary>
    public IQueryable<Partner> ApplyOrgUnitFilter(IQueryable<Partner> query, DbContext context)
    {
        if (_orgUnitHierarchyIds == null || _orgUnitHierarchyIds.Count == 0)
        {
            return query.Where(p => false); // No results for security
        }

        // Pre-materialize the partner IDs that match the org unit criteria to avoid nested query issues
        var validPartnerIds = context.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                _orgUnitHierarchyIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToList(); // Materialize the IDs first

        // Now filter the partners using the materialized IDs
        return query.Where(partner => validPartnerIds.Contains(partner.Id));
    }
}