namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Specification to filter contacts by organizational unit hierarchy through their partner
/// Uses manual joins to efficiently filter at the database level without navigation properties
/// </summary>
public class ContactByOrgUnitHierarchySpecification : BaseSpecification<Contact>
{
    private readonly List<int> _orgUnitHierarchyIds;
    
    public ContactByOrgUnitHierarchySpecification(List<int> orgUnitHierarchyIds)
        : base(BuildCriteria(orgUnitHierarchyIds))
    {
        _orgUnitHierarchyIds = orgUnitHierarchyIds ?? new List<int>();
        // Include related entities
        AddInclude(c => c.Partner);
    }

    private static Expression<Func<Contact, bool>> BuildCriteria(List<int> orgUnitHierarchyIds)
    {
        if (orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0)
        {
            // If no org units specified, return no results for security
            return c => false;
        }

        // Filter by Partner existence - the actual org unit filtering will be done via manual join
        return c => c.Partner != null;
    }
    
    /// <summary>
    /// Apply manual join filtering to the query for efficient database-level filtering
    /// This should be called by the repository/manager when applying the specification
    /// </summary>
    public IQueryable<Contact> ApplyOrgUnitFilter(IQueryable<Contact> query, DbContext context)
    {
        if (_orgUnitHierarchyIds == null || _orgUnitHierarchyIds.Count == 0)
        {
            return query.Where(c => false); // No results for security
        }

        // Pre-materialize the partner IDs that match the org unit criteria to avoid nested query issues
        var validPartnerIds = context.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                _orgUnitHierarchyIds.Contains(orgRel.OrganizationHierarchyId))
            .Select(orgRel => orgRel.EntityId)
            .ToList(); // Materialize the IDs first

        // Now filter the contacts using the materialized partner IDs
        return query.Where(contact => 
            validPartnerIds.Contains(contact.PartnerId));
    }
}