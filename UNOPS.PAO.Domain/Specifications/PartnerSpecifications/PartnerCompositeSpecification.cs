namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// A composite specification that allows filtering partners by multiple criteria
/// </summary>
public class PartnerCompositeSpecification : GenericCompositeSpecification<Partner, IPartnerSearchFilter>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for partners
    /// </summary>
    /// <param name="filter">The filter containing all search criteria</param>
    public PartnerCompositeSpecification(IPartnerSearchFilter filter)
        : base(filter)
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
        
        // Default ordering is by name
        ApplyOrderBy(p => p.Name);
    }
} 