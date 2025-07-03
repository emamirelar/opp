namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Specification to filter UNOPS contacts by organizational unit hierarchy through their partner
/// </summary>
public class UNOPSContactByOrgUnitHierarchySpecification : BaseSpecification<UNOPSContact>
{

    public UNOPSContactByOrgUnitHierarchySpecification(List<int> orgUnitHierarchyIds)
        : base(BuildCriteria(orgUnitHierarchyIds))
    {
        // Include related entities
        AddInclude(c => c.Partner);
        AddInclude($"{nameof(UNOPSContact.Partner)}.{nameof(UNOPSPartner.PartnerOffice)}");
    }

    private static Expression<Func<UNOPSContact, bool>> BuildCriteria(List<int> orgUnitHierarchyIds)
    {
        if (orgUnitHierarchyIds == null || orgUnitHierarchyIds.Count == 0)
        {
            // If no org units specified, return no results for security
            return c => false;
        }

        // Filter by Partner's PartnerOfficeId in the hierarchy
        return c => c.Partner != null && 
                   c.Partner.PartnerOfficeId.HasValue && 
                   orgUnitHierarchyIds.Contains(c.Partner.PartnerOfficeId.Value);
    }
}