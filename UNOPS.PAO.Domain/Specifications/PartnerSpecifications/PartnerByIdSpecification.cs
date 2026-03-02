namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by ID
/// </summary>
public class PartnerByIdSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by ID
    /// </summary>
    /// <param name="id">The partner ID to filter by</param>
    public PartnerByIdSpecification(int id)
        : base(p => p.Id == id)
    {
        // Include related entities
        AddInclude(p => p.Documents!);
    }
} 