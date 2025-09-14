namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using System.Linq.Expressions;

/// <summary>
/// Composite specification for partners that includes organizational unit hierarchy filtering with relations
/// </summary>
public class PartnerCompositeWithOrgUnitAndRelationsSpecification : BaseCompositeSpecification<Partner>
{
    public PartnerCompositeWithOrgUnitAndRelationsSpecification(
        IPartnerSearchFilter filter, 
        List<int> orgUnitHierarchyIds,
        List<string> orgUnitUserIds)
        : base(BuildCombinedCriteria(filter, orgUnitHierarchyIds, orgUnitUserIds))
    {
        // Create base specification to copy includes
        var baseSpec = new PartnerCompositeSpecification(filter);
        
        // Include related entities for org unit relations
        AddInclude(p => p.Contacts);
        AddInclude($"{nameof(Partner.Contacts)}.{nameof(Contact.Interactions)}");
        AddInclude($"{nameof(Partner.Contacts)}.{nameof(Contact.Interactions)}.{nameof(Interaction.InteractionUsers)}");
        
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
    
    private static Expression<Func<Partner, bool>> BuildCombinedCriteria(
        IPartnerSearchFilter filter, 
        List<int> orgUnitHierarchyIds,
        List<string> orgUnitUserIds)
    {
        // Create base composite specification
        var baseSpec = new PartnerCompositeSpecification(filter);
        
        // Create org unit with relations specification
        var orgUnitSpec = new PartnerByOrgUnitWithRelationsSpecification(orgUnitHierarchyIds, orgUnitUserIds);
        
        // Combine the criteria using the base class method
        return CombineExpressions(baseSpec.Criteria, orgUnitSpec.Criteria);
    }
}