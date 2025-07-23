namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Composite specification for UNOPS contacts that includes organizational unit hierarchy filtering
/// </summary>
public class UNOPSContactCompositeWithOrgUnitSpecification : BaseCompositeSpecification<UNOPSContact>
{
    public UNOPSContactCompositeWithOrgUnitSpecification(IContactSearchFilter filter, List<int> orgUnitHierarchyIds)
        : base(BuildCombinedCriteria(filter, orgUnitHierarchyIds))
    {
        // Create base specification to copy includes
        var baseSpec = new UNOPSContactCompositeSpecification(filter);
        
        // Include related entities
        AddInclude(c => c.Partner);
        AddInclude($"{nameof(UNOPSContact.Partner)}.{nameof(UNOPSPartner.OrganizationUnitRelationships)}");
        AddInclude($"{nameof(UNOPSContact.Partner)}.{nameof(UNOPSPartner.OrganizationUnitRelationships)}.{nameof(OrganizationUnitRelationship.OrganizationHierarchy)}");
        
        // Copy includes from base specification
        foreach (var include in baseSpec.Includes)
        {
            AddInclude(include);
        }
        
        // Copy include strings from base specification
        foreach (var includeString in baseSpec.IncludeStrings)
        {
            AddInclude(includeString);
        }
    }
    
    private static Expression<Func<UNOPSContact, bool>> BuildCombinedCriteria(
        IContactSearchFilter filter, 
        List<int> orgUnitHierarchyIds)
    {
        // Create base composite specification
        var baseSpec = new UNOPSContactCompositeSpecification(filter);
        
        // Create org unit hierarchy specification
        var orgUnitSpec = new UNOPSContactByOrgUnitHierarchySpecification(orgUnitHierarchyIds);
        
        // Combine the criteria using the base class method
        return CombineExpressions(baseSpec.Criteria, orgUnitSpec.Criteria);
    }
}