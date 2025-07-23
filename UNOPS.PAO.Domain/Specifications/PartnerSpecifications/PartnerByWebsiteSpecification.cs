namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by website
/// </summary>
public class PartnerByWebsiteSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by website
    /// </summary>
    /// <param name="website">The website to filter by</param>
    public PartnerByWebsiteSpecification(string website)
        : base(p => p.Website != null && p.Website.ToLower().Contains(website.ToLower()))
    {
        // Include related entities
        AddInclude(p => p.OrganizationUnitRelationships);
        AddInclude("OrganizationUnitRelationships.OrganizationHierarchy");
    }
} 