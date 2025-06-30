namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// Specification that filters UNOPS partners by status
/// </summary>
public class UNOPSPartnerByStatusSpecification : BaseSpecification<UNOPSPartner>
{
    /// <summary>
    /// Creates a specification that filters UNOPS partners by status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    public UNOPSPartnerByStatusSpecification(string? status)
        : base(string.IsNullOrEmpty(status) ? p => true : p => p.Status == status)
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
    }
}