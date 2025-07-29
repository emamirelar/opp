namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by phone number
/// </summary>
public class PartnerByPhoneSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by phone number
    /// </summary>
    /// <param name="phone">The phone number to filter by</param>
    public PartnerByPhoneSpecification(string phone)
        : base(p => p.Phone != null && p.Phone.Contains(phone))
    {
        // Include related entities
    }
} 