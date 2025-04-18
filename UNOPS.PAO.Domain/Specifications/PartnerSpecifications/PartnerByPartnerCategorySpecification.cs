namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by partner category
/// </summary>
public class PartnerByPartnerCategorySpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by partner category ID
    /// </summary>
    /// <param name="partnerCategoryId">The partner category ID to filter by</param>
    public PartnerByPartnerCategorySpecification(int partnerCategoryId)
        : base(p => p.PartnerCategoryId == partnerCategoryId)
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
        AddInclude(p => p.PartnerCategory);
    }
} 