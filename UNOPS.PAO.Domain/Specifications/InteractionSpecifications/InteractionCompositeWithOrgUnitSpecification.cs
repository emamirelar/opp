namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using System.Linq.Expressions;

/// <summary>
/// Composite specification for interactions that includes organizational unit hierarchy filtering
/// </summary>
public class InteractionCompositeWithOrgUnitSpecification : BaseCompositeSpecification<Interaction>
{
    public InteractionCompositeWithOrgUnitSpecification(IInteractionSearchFilter filter, List<int> orgUnitHierarchyIds)
        : base(BuildCombinedCriteria(filter, orgUnitHierarchyIds))
    {
        // Create base specification to copy includes
        var baseSpec = new InteractionCompositeSpecification(filter);
        
        // Include related entities
        AddInclude(i => i.OrgUnit);
        
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
    
    private static Expression<Func<Interaction, bool>> BuildCombinedCriteria(
        IInteractionSearchFilter filter, 
        List<int> orgUnitHierarchyIds)
    {
        // Create base composite specification
        var baseSpec = new InteractionCompositeSpecification(filter);
        
        // Create org unit hierarchy specification
        var orgUnitSpec = new InteractionByOrgUnitHierarchySpecification(orgUnitHierarchyIds);
        
        // Combine the criteria using the base class method
        return CombineExpressions(baseSpec.Criteria, orgUnitSpec.Criteria);
    }
}