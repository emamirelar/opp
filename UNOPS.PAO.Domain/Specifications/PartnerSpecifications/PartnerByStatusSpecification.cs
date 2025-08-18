namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by status
/// Updated to use new SystemStatus enum field
/// </summary>
public class PartnerByStatusSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    public PartnerByStatusSpecification(string status)
        : base(p => p.Status.ToString() == status)
    {
        // Include related entities
    }
} 