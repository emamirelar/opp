namespace UNOPS.PAO.UNOPSDomain.Specifications;

using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// A composite specification for UNOPS contacts that supports MyOfficeOnly filtering
/// </summary>
public class UNOPSContactCompositeWithMyOfficeSpecification : UserContextCompositeSpecification<UNOPSContact>
{
    public UNOPSContactCompositeWithMyOfficeSpecification(IContactSearchFilter filter, string? userOrgUnit)
        : base(
            baseSpecification: CreateBaseSpecification(filter),
            myOfficeSpecification: string.IsNullOrEmpty(userOrgUnit) ? null : new UNOPSContactByMyOfficeSpecification(userOrgUnit),
            applyMyOffice: filter.MyOfficeOnly && !string.IsNullOrEmpty(userOrgUnit))
    {
    }

    private static ISpecification<UNOPSContact> CreateBaseSpecification(IContactSearchFilter filter)
    {
        // For now, create a simple specification based on title filter
        // This can be expanded later with more complex filtering
        return new UNOPSContactByTitleSpecification(filter.Title);
    }
}