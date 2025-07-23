namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Domain.Specifications.PartnerSpecifications;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Composite specification for UNOPS partners that includes organizational unit hierarchy filtering
/// </summary>
public class UNOPSPartnerCompositeWithOrgUnitSpecification : BaseCompositeSpecification<UNOPSPartner>
{
    public UNOPSPartnerCompositeWithOrgUnitSpecification(IPartnerSearchFilter filter, List<int> orgUnitHierarchyIds)
        : base(BuildCombinedCriteria(filter, orgUnitHierarchyIds))
    {
        // Create base specification to copy includes
        var baseSpec = new UNOPSPartnerCompositeSpecification(filter);
        
        // Include related entities
        AddInclude(p => p.OrganizationUnitRelationships);
        AddInclude("OrganizationUnitRelationships.OrganizationHierarchy");
        
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
    
    private static Expression<Func<UNOPSPartner, bool>> BuildCombinedCriteria(
        IPartnerSearchFilter filter, 
        List<int> orgUnitHierarchyIds)
    {
        // Create base composite specification
        var baseSpec = new UNOPSPartnerCompositeSpecification(filter);
        
        // Create org unit hierarchy specification
        var orgUnitSpec = new UNOPSPartnerByOrgUnitHierarchySpecification(orgUnitHierarchyIds);
        
        // Combine the criteria using the base class method
        return CombineExpressions(baseSpec.Criteria, orgUnitSpec.Criteria);
    }
}