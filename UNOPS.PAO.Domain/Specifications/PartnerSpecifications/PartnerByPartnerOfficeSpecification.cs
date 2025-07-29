namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Specification that filters partners by organization hierarchy
/// Uses manual joins to efficiently filter at the database level without navigation properties
/// </summary>
public class PartnerByPartnerOfficeSpecification : BaseSpecification<Partner>
{
    private readonly int _organizationHierarchyId;
    
    /// <summary>
    /// Creates a specification that filters partners by organization hierarchy ID
    /// </summary>
    /// <param name="organizationHierarchyId">The organization hierarchy ID to filter by</param>
    public PartnerByPartnerOfficeSpecification(int organizationHierarchyId)
        : base(p => true) // Basic filtering - org unit filtering will be done via manual join
    {
        _organizationHierarchyId = organizationHierarchyId;
        // Include related entities
    }
    
    /// <summary>
    /// Apply manual join filtering to the query for efficient database-level filtering
    /// This should be called by the repository/manager when applying the specification
    /// </summary>
    public IQueryable<Partner> ApplyOrgUnitFilter(IQueryable<Partner> query, DbContext context)
    {
        // Pre-materialize the partner IDs that match the org unit criteria to avoid nested query issues
        var validPartnerIds = context.Set<OrganizationUnitRelationship>()
            .Where(orgRel => 
                orgRel.EntityType == "Partner" && 
                orgRel.OrganizationHierarchyId == _organizationHierarchyId)
            .Select(orgRel => orgRel.EntityId)
            .ToList(); // Materialize the IDs first

        // Now filter the partners using the materialized IDs
        return query.Where(partner => validPartnerIds.Contains(partner.Id));
    }
} 