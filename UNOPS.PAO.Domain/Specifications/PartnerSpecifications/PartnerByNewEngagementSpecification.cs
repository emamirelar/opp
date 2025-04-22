namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by new engagement status
/// </summary>
public class PartnerByNewEngagementSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by new engagement status
    /// </summary>
    /// <param name="newEngagement">The new engagement status to filter by</param>
    public PartnerByNewEngagementSpecification(string newEngagement)
        : base(p => p.NewEngagement == newEngagement)
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
        AddInclude(p => p.PartnerCategory);
    }
} 