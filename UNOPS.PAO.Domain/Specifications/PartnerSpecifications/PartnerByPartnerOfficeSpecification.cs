namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by partner office
/// </summary>
public class PartnerByPartnerOfficeSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by partner office ID
    /// </summary>
    /// <param name="partnerOfficeId">The partner office ID to filter by</param>
    public PartnerByPartnerOfficeSpecification(int partnerOfficeId)
        : base(p => p.PartnerOfficeId == partnerOfficeId)
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
        AddInclude(p => p.PartnerCategory);
    }
} 