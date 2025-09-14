namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by website
/// NOTE: Website field is no longer part of the Partner entity.
/// This specification now returns all partners for backward compatibility.
/// Consider removing if website filtering is not needed.
/// </summary>
public class PartnerByWebsiteSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by website
    /// </summary>
    /// <param name="website">The website to filter by (deprecated - not used)</param>
    public PartnerByWebsiteSpecification(string website)
        : base(p => true) // Return all partners as website field no longer exists
    {
        // Include related entities
    }
} 