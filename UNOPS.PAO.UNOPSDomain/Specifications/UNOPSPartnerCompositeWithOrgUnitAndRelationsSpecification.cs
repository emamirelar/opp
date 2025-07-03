namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Linq.Expressions;

/// <summary>
/// Composite specification for UNOPS partners that includes organizational unit hierarchy filtering with relations
/// </summary>
public class UNOPSPartnerCompositeWithOrgUnitAndRelationsSpecification : BaseCompositeSpecification<UNOPSPartner>
{
    public UNOPSPartnerCompositeWithOrgUnitAndRelationsSpecification(
        IPartnerSearchFilter filter, 
        List<int> orgUnitHierarchyIds,
        List<int> orgUnitUserIds)
        : base(BuildCombinedCriteria(filter, orgUnitHierarchyIds, orgUnitUserIds))
    {
        // Create base specification to copy includes
        var baseSpec = new UNOPSPartnerCompositeSpecification(filter);
        
        // Include related entities for org unit relations
        AddInclude(p => p.PartnerOffice);
        AddInclude(p => p.Contacts);
        AddInclude($"{nameof(UNOPSPartner.Contacts)}.{nameof(UNOPSContact.Interactions)}");
        AddInclude($"{nameof(UNOPSPartner.Contacts)}.{nameof(UNOPSContact.Interactions)}.{nameof(UNOPSInteraction.InteractionUsers)}");
        
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
        List<int> orgUnitHierarchyIds,
        List<int> orgUnitUserIds)
    {
        // Create base composite specification
        var baseSpec = new UNOPSPartnerCompositeSpecification(filter);
        
        // Create org unit with relations specification
        var orgUnitSpec = new UNOPSPartnerByOrgUnitWithRelationsSpecification(orgUnitHierarchyIds, orgUnitUserIds);
        
        // Combine the criteria using the base class method
        return CombineExpressions(baseSpec.Criteria, orgUnitSpec.Criteria);
    }
}