namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by short name
/// Updated to use new PartnerShortDescription field
/// </summary>
public class PartnerByShortNameSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by short name
    /// </summary>
    /// <param name="shortName">The short name to filter by</param>
    public PartnerByShortNameSpecification(string shortName)
        : base(p => p.PartnerShortDescription != null && p.PartnerShortDescription.ToLower().Contains(shortName.ToLower()))
    {
        // Include related entities
    }
} 