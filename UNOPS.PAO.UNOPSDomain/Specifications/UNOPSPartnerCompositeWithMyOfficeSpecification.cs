namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// A composite specification for UNOPS partners that supports MyOfficeOnly filtering
/// </summary>
public class UNOPSPartnerCompositeWithMyOfficeSpecification : UserContextCompositeSpecification<UNOPSPartner>
{
    public UNOPSPartnerCompositeWithMyOfficeSpecification(IPartnerSearchFilter filter, string? userOrgUnit)
        : base(
            baseSpecification: CreateBaseSpecification(filter),
            myOfficeSpecification: string.IsNullOrEmpty(userOrgUnit) ? null : new UNOPSPartnerByMyOfficeSpecification(userOrgUnit),
            applyMyOffice: filter.MyOfficeOnly && !string.IsNullOrEmpty(userOrgUnit))
    {
    }

    private static ISpecification<UNOPSPartner> CreateBaseSpecification(IPartnerSearchFilter filter)
    {
        // For now, create a simple specification based on status filter
        // This can be expanded later with more complex filtering
        return new UNOPSPartnerByStatusSpecification(filter.Status);
    }
}