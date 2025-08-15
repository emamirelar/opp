namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by new engagement status
/// NOTE: NewEngagement field is no longer part of the Partner entity.
/// This specification now filters by CanCreateNewOpportunities field.
/// </summary>
public class PartnerByNewEngagementSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by new engagement capability
    /// </summary>
    /// <param name="newEngagement">The new engagement status to filter by</param>
    public PartnerByNewEngagementSpecification(string newEngagement)
        : base(p => newEngagement.ToLower() == "yes" ? p.CanCreateNewOpportunities : !p.CanCreateNewOpportunities)
    {
        // Include related entities
    }
} 