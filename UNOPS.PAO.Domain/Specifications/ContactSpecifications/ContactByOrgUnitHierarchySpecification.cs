namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using UNOPS.PAO.Domain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter contacts by organizational unit hierarchy through their partner
/// </summary>
public class ContactByOrgUnitHierarchySpecification : BaseSpecification<Contact>
{
    public ContactByOrgUnitHierarchySpecification(List<int> orgUnitHierarchyIds)
        : base(BuildCriteria(orgUnitHierarchyIds))
    {
        // Include related entities
        AddInclude(c => c.Partner);
        AddInclude($"{nameof(Contact.Partner)}.{nameof(Partner.OrganizationUnitRelationships)}");
        AddInclude($"{nameof(Contact.Partner)}.{nameof(Partner.OrganizationUnitRelationships)}.{nameof(OrganizationUnitRelationship.OrganizationHierarchy)}");
    }

    private static Expression<Func<Contact, bool>> BuildCriteria(List<int> orgUnitHierarchyIds)
    {
        if (orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0)
        {
            // If no org units specified, return no results for security
            return c => false;
        }

        // Filter by Partner's OrganizationUnitRelationships in the hierarchy
        return c => c.Partner != null && 
                   c.Partner.OrganizationUnitRelationships.Any(r => 
                       orgUnitHierarchyIds.Contains(r.OrganizationHierarchyId));
    }
}