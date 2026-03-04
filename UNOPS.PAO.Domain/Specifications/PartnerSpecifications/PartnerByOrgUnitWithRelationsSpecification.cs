namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification to filter partners by organizational unit hierarchy including indirect relations through contacts
/// Uses manual joins to efficiently filter at the database level without navigation properties
/// </summary>
public class PartnerByOrgUnitWithRelationsSpecification : BaseSpecification<Partner>
{
    private readonly List<int> _orgUnitHierarchyIds;
    //private readonly List<string> _orgUnitUserIds;
    
    public PartnerByOrgUnitWithRelationsSpecification(
        List<int> orgUnitHierarchyIds, 
        List<string> orgUnitUserIds)
        : base(BuildCriteria(orgUnitHierarchyIds, ConvertUserIdsToIntegers(orgUnitUserIds)))
    {
        _orgUnitHierarchyIds = orgUnitHierarchyIds ?? new List<int>();
        //_orgUnitUserIds = orgUnitUserIds;
        // Include related entities for the query
        AddInclude(p => p.Contacts);
        AddInclude($"{nameof(Partner.Contacts)}.{nameof(Contact.Interactions)}");
        AddInclude($"{nameof(Partner.Contacts)}.{nameof(Contact.Interactions)}.{nameof(Interaction.InteractionContacts)}");
        AddInclude($"{nameof(Partner.Contacts)}.{nameof(Contact.Interactions)}.{nameof(Interaction.InteractionUsers)}");
    }

    /// <summary>
    /// Converts string user IDs to integers, filtering out invalid values
    /// </summary>
    private static List<int> ConvertUserIdsToIntegers(List<string> orgUnitUserIds)
    {
        if (orgUnitUserIds == null)
            return new List<int>();
            
        return orgUnitUserIds
            .Where(id => int.TryParse(id, out _))
            .Select(id => int.Parse(id))
            .ToList();
    }

    private static Expression<Func<Partner, bool>> BuildCriteria(
        List<int> orgUnitHierarchyIds, 
        List<int> orgUnitUserIds)
    {
        // If both lists are empty, return no results for security
        if ((orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0) && 
            (orgUnitUserIds == null || orgUnitUserIds.Count == 0))
        {
            return p => false;
        }

        // Build the criteria expression
        return p => 
            // Case 1: Partner directly linked to org unit hierarchy via OrganizationUnitRelationships
            // Note: OrganizationUnitRelationships filtering moved to post-query processing
            (orgUnitHierarchyIds != null && orgUnitHierarchyIds.Count > 0)
            ||
            // Case 2: Partner has contacts with interactions involving org unit users
            (orgUnitUserIds != null && 
             orgUnitUserIds.Count > 0 && 
             (p.Contacts ?? Enumerable.Empty<Contact>()).Any(c => 
                (c.Interactions ?? Enumerable.Empty<Interaction>()).Any(i => 
                    (i.InteractionUsers ?? Enumerable.Empty<InteractionUser>()).Any(iu => 
                        orgUnitUserIds.Contains(iu.UserId)))));
    }
    
    /// <summary>
    /// Apply manual join filtering for organization unit hierarchy
    /// This should be called by the repository/manager when applying the specification
    /// </summary>
    public IQueryable<Partner> ApplyOrgUnitFilter(IQueryable<Partner> query, DbContext context)
    {
        if (_orgUnitHierarchyIds == null || _orgUnitHierarchyIds.Count == 0)
        {
            return query;
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