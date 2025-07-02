namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// A composite specification that allows filtering UNOPS partners by multiple criteria
/// </summary>
public class UNOPSPartnerCompositeSpecification : GenericCompositeSpecification<UNOPSPartner, IPartnerSearchFilter>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for UNOPS partners
    /// </summary>
    /// <param name="filter">The filter containing all search criteria</param>
    public UNOPSPartnerCompositeSpecification(IPartnerSearchFilter filter)
        : base(filter)
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
        AddInclude(p => p.PartnerGroup);
        AddInclude(p => p.Projects);
        
        // Default ordering is by name
        ApplyOrderBy(p => p.Name);
    }
}