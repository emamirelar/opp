namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by name
/// Updated to use new PartnerDescription field
/// </summary>
public class PartnerByNameSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by name
    /// </summary>
    /// <param name="name">The partner name to filter by</param>
    public PartnerByNameSpecification(string name)
        : base(p => p.Name != null && p.Name.ToLower().Contains(name.ToLower()))
    {
        // Include related entities
    }
} 