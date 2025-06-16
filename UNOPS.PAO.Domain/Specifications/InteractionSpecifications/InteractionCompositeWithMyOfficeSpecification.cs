namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// A composite specification for interactions that supports MyOfficeOnly filtering
/// </summary>
public class InteractionCompositeWithMyOfficeSpecification : UserContextCompositeSpecification<Interaction>
{
    public InteractionCompositeWithMyOfficeSpecification(IInteractionSearchFilter filter, string? userOrgUnit)
        : base(
            baseSpecification: new InteractionCompositeSpecification(filter),
            myOfficeSpecification: string.IsNullOrEmpty(userOrgUnit) ? null : new InteractionByMyOfficeSpecification(userOrgUnit),
            applyMyOffice: filter.MyOfficeOnly && !string.IsNullOrEmpty(userOrgUnit))
    {
    }
}