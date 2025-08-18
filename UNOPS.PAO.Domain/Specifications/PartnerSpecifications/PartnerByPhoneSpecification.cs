namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by phone number
/// NOTE: Phone field is no longer part of the Partner entity.
/// This specification now returns all partners for backward compatibility.
/// Consider removing if phone filtering is not needed.
/// </summary>
public class PartnerByPhoneSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by phone number
    /// </summary>
    /// <param name="phone">The phone number to filter by (deprecated - not used)</param>
    public PartnerByPhoneSpecification(string phone)
        : base(p => true) // Return all partners as phone field no longer exists
    {
        // Include related entities
    }
} 