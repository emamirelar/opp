namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Composite specification for interactions that includes organizational unit hierarchy filtering
/// Uses manual joins to efficiently filter at the database level without navigation properties
/// </summary>
public class InteractionCompositeWithOrgUnitSpecification : BaseCompositeSpecification<Interaction>
{
    private readonly List<int> _orgUnitHierarchyIds;

    public InteractionCompositeWithOrgUnitSpecification(IInteractionSearchFilter filter, List<int> orgUnitHierarchyIds)
        : base(BuildCombinedCriteria(filter, orgUnitHierarchyIds))
    {
        _orgUnitHierarchyIds = orgUnitHierarchyIds ?? new List<int>();
        
        // Create base specification to copy includes
        var baseSpec = new InteractionCompositeSpecification(filter);
        
        // Copy includes from base specification (but not OrgUnit)
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
    
    /// <summary>
    /// Apply manual join filtering for organization hierarchy
    /// This delegates to the underlying org unit specification for the actual filtering logic
    /// </summary>
    public IQueryable<Interaction> ApplyOrgUnitFilter(IQueryable<Interaction> query, DbContext context)
    {
        // Create the org unit specification and delegate to it
        var orgUnitSpec = new InteractionByOrgUnitHierarchySpecification(_orgUnitHierarchyIds);
        return orgUnitSpec.ApplyOrgUnitFilter(query, context);
    }
}