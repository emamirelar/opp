namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by address fields
/// NOTE: Address fields are no longer part of the Partner entity.
/// This specification now returns all partners for backward compatibility.
/// Consider removing if address filtering is not needed.
/// </summary>
public class PartnerByAddressSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by city
    /// </summary>
    /// <param name="city">The city to filter by (deprecated - not used)</param>
    public PartnerByAddressSpecification(string? city = null, string? stateProvince = null, string? postalCode = null, string? country = null)
        : base(p => true) // Return all partners as address fields no longer exist
    {
        // Include related entities
    }
} 