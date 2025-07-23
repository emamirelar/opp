namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter partners by organizational unit hierarchy including indirect relations through contacts
/// </summary>
public class PartnerByOrgUnitWithRelationsSpecification : BaseSpecification<Partner>
{
    public PartnerByOrgUnitWithRelationsSpecification(
        List<int> orgUnitHierarchyIds, 
        List<int> orgUnitUserIds)
        : base(BuildCriteria(orgUnitHierarchyIds, orgUnitUserIds))
    {
        // Include related entities for the query
        AddInclude(p => p.OrganizationUnitRelationships);
        AddInclude("OrganizationUnitRelationships.OrganizationHierarchy");
        AddInclude(p => p.Contacts);
        AddInclude($"{nameof(Partner.Contacts)}.{nameof(Contact.Interactions)}");
        AddInclude($"{nameof(Partner.Contacts)}.{nameof(Contact.Interactions)}.{nameof(Interaction.InteractionContacts)}");
        AddInclude($"{nameof(Partner.Contacts)}.{nameof(Contact.Interactions)}.{nameof(Interaction.InteractionUsers)}");
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
            (orgUnitHierarchyIds != null && 
             orgUnitHierarchyIds.Count > 0 && 
             p.OrganizationUnitRelationships.Any(r => 
                orgUnitHierarchyIds.Contains(r.OrganizationHierarchyId)))
            ||
            // Case 2: Partner has contacts with interactions involving org unit users
            (orgUnitUserIds != null && 
             orgUnitUserIds.Count > 0 && 
             p.Contacts.Any(c => 
                c.Interactions.Any(i => 
                    i.InteractionUsers.Any(iu => 
                        orgUnitUserIds.Contains(iu.UserId)))));
    }
}