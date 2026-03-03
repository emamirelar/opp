namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Composite specification for UNOPS contacts that includes organizational unit hierarchy filtering
/// </summary>
public class UNOPSContactCompositeWithOrgUnitSpecification : BaseCompositeSpecification<UNOPSContact>
{
    private readonly List<int> _orgUnitHierarchyIds;
    
    public UNOPSContactCompositeWithOrgUnitSpecification(IContactSearchFilter filter, List<int> orgUnitHierarchyIds)
        : base(BuildCombinedCriteria(filter, orgUnitHierarchyIds))
    {
        _orgUnitHierarchyIds = orgUnitHierarchyIds ?? new List<int>();
        
        // Create base specification to copy includes
        var baseSpec = new UNOPSContactCompositeSpecification(filter);
        
        // Include related entities
        AddInclude(c => c.Partner!);
        
        // Copy includes from base specification
        foreach (var include in baseSpec.Includes)
        {
            if (include != null)
            {
                AddInclude(include);
            }
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
    
    /// <summary>
    /// Apply manual join filtering to the query for efficient database-level filtering
    /// This delegates to the underlying org unit specification for the actual filtering logic
    /// </summary>
    public IQueryable<UNOPSContact> ApplyOrgUnitFilter(IQueryable<UNOPSContact> query, DbContext context)
    {
        // Create the org unit specification and delegate to it
        var orgUnitSpec = new UNOPSContactByOrgUnitHierarchySpecification(_orgUnitHierarchyIds);
        return orgUnitSpec.ApplyOrgUnitFilter(query, context);
    }
}